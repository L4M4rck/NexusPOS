using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NexusPOS.Application.Checkout;
using NexusPOS.Application.Pricing;
using NexusPOS.Domain.Entities;
using NexusPOS.Domain.Enums;
using NexusPOS.Domain.Exceptions;
using NexusPOS.Infrastructure.Persistence;

namespace NexusPOS.Infrastructure.Sales;
// Orquesta el caso de uso principal: valida la solicitud, usa precios reales,
// reserva inventario, procesa pago y persiste venta, detalle y factura.
internal sealed class CheckoutService(
    NexusPosDbContext dbContext,
    IPricingService pricingService,
    IPaymentGateway paymentGateway) : ICheckoutService
{
    // Ejecuta la compra completa para el cliente asociado al userId autenticado.
    public async Task<SaleResponse> CheckoutAsync(int userId, CheckoutRequest request, CancellationToken cancellationToken)
    {
        ValidateRequest(request);

        // El cliente se resuelve desde el JWT; el navegador no puede escoger otro CustomerId.
        var customer = await dbContext.Customers.Include(x => x.User)
            .SingleOrDefaultAsync(x => x.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("No existe un perfil de cliente para este usuario.");

        // Responder con la venta existente hace seguro repetir una petición ya confirmada.
        var existing = await FindByIdempotencyKeyAsync(customer.Id, request.IdempotencyKey, cancellationToken);
        if (existing is not null)
        {
            return existing.ToSaleResponse();
        }

        // Si el mismo producto llega repetido, se consolida para validar y descontar una sola cantidad total.
        var requestedItems = request.Items.GroupBy(x => x.ProductId)
            .Select(group => new CheckoutItemRequest(group.Key, group.Sum(item => item.Quantity)))
            .ToArray();
        var ids = requestedItems.Select(x => x.ProductId).ToArray();
        // Precio, nombre, estado y stock proceden de MySQL; nunca se confía en valores del frontend.
        var products = await dbContext.Products.AsNoTracking()
            .Where(x => ids.Contains(x.Id) && x.IsActive)
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        if (products.Count != ids.Length)
        {
            throw new BusinessException("Uno o más productos no existen o están inactivos.");
        }

        foreach (var item in requestedItems)
        {
            var product = products[item.ProductId];
            if (product.Stock < item.Quantity)
            {
                throw new ConflictException($"Stock insuficiente para {product.Name}. Disponible: {product.Stock}.", "insufficient-stock");
            }
        }

        // PricingService recibe exclusivamente precios que el servidor acaba de consultar.
        var pricing = pricingService.Calculate(requestedItems.Select(item =>
        {
            var product = products[item.ProductId];
            return new PricingLine(product.Id, product.Name, item.Quantity, product.Price);
        }));

        // La base InMemory usada por algunas pruebas no soporta transacciones relacionales.
        IDbContextTransaction? transaction = null;
        if (dbContext.Database.IsRelational())
        {
            transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        }

        try
        {
            // Se revisa de nuevo dentro de la transacción para cerrar la ventana entre
            // la primera consulta y dos solicitudes concurrentes con la misma clave.
            existing = await FindByIdempotencyKeyAsync(customer.Id, request.IdempotencyKey, cancellationToken);
            if (existing is not null)
            {
                if (transaction is not null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                }

                return existing.ToSaleResponse();
            }

            await ReserveInventoryAsync(requestedItems, products, cancellationToken);

            // El gateway es una abstracción: hoy es simulado y puede reemplazarse sin cambiar este flujo.
            var paymentResult = await paymentGateway.ProcessAsync(
                new PaymentRequest(pricing.Total, "COP", request.PaymentMethod, request.IdempotencyKey), cancellationToken);
            if (!paymentResult.IsApproved)
            {
                throw new ConflictException(paymentResult.DeclineReason ?? "El pago fue rechazado.", "payment-declined");
            }

            // SaleItem guarda snapshots de nombre y precio para que el histórico no cambie
            // al editar posteriormente el catálogo.
            var sale = new Sale
            {
                CustomerId = customer.Id,
                IdempotencyKey = request.IdempotencyKey.Trim(),
                InvoiceNumber = $"TMP-{Guid.NewGuid():N}"[..20],
                Subtotal = pricing.Subtotal,
                Tax = pricing.Tax,
                Discount = pricing.Discount,
                Total = pricing.Total,
                Status = SaleStatus.Completed,
                Items = pricing.Items.Select(item => new SaleItem
                {
                    ProductId = item.ProductId,
                    ProductNameSnapshot = item.ProductName,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Subtotal = item.Subtotal
                }).ToArray()
            };

            dbContext.Sales.Add(sale);

            // El primer guardado obtiene Sale.Id autoincremental, necesario para el consecutivo FV.
            await dbContext.SaveChangesAsync(cancellationToken);

            var invoiceNumber = $"FV-{DateTime.UtcNow.Year}-{sale.Id:D6}";
            sale.InvoiceNumber = invoiceNumber;
            // Pago y factura son entidades separadas porque representan responsabilidades distintas.
            sale.Payment = new Payment
            {
                Provider = "MockPaymentGateway",
                ProviderReference = paymentResult.ProviderReference,
                Amount = sale.Total,
                Status = PaymentStatus.Approved
            };
            sale.Invoice = new Invoice
            {
                Number = invoiceNumber,
                CustomerNameSnapshot = $"{customer.User.FirstName} {customer.User.LastName}",
                CustomerDocumentSnapshot = customer.DocumentNumber
            };
            await dbContext.SaveChangesAsync(cancellationToken);

            if (transaction is not null)
            {
                // Solo después de persistir todo se confirma también el descuento de inventario.
                await transaction.CommitAsync(cancellationToken);
            }

            return sale.ToSaleResponse();
        }
        catch
        {
            if (transaction is not null)
            {
                // Se intenta el rollback aunque la petición original ya haya sido cancelada.
                await transaction.RollbackAsync(CancellationToken.None);
            }

            throw;
        }
        finally
        {
            if (transaction is not null)
            {
                await transaction.DisposeAsync();
            }
        }
    }
    // Descuenta cada producto de forma condicional para impedir stock negativo
    // aunque dos compradores intenten adquirir la última unidad simultáneamente.
    private async Task ReserveInventoryAsync(
        IReadOnlyList<CheckoutItemRequest> requestedItems,
        IReadOnlyDictionary<int, Product> products,
        CancellationToken cancellationToken)
    {
        foreach (var item in requestedItems)
        {
            if (dbContext.Database.IsRelational())
            {
                // El WHERE Stock >= Quantity y la resta ocurren como una sola operación SQL atómica.
                var updated = await dbContext.Products
                    .Where(x => x.Id == item.ProductId && x.IsActive && x.Stock >= item.Quantity)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.Stock, x => x.Stock - item.Quantity), cancellationToken);
                if (updated != 1)
                {
                    throw new ConflictException($"Stock insuficiente para {products[item.ProductId].Name}.", "insufficient-stock");
                }
            }
            else
            {
                // Rama equivalente para EF InMemory durante pruebas de integración.
                var tracked = await dbContext.Products.SingleAsync(x => x.Id == item.ProductId, cancellationToken);
                if (tracked.Stock < item.Quantity)
                {
                    throw new ConflictException($"Stock insuficiente para {tracked.Name}.", "insufficient-stock");
                }

                tracked.Stock -= item.Quantity;
            }
        }
    }

    // Recupera la venta producida previamente por la misma clave idempotente.
    private Task<Sale?> FindByIdempotencyKeyAsync(int customerId, string key, CancellationToken cancellationToken) =>
        dbContext.Sales.AsNoTracking().Include(x => x.Items).Include(x => x.Invoice)
            .SingleOrDefaultAsync(x => x.CustomerId == customerId && x.IdempotencyKey == key.Trim(), cancellationToken);

    // Rechaza solicitudes vacías, identificadores inválidos y cantidades no positivas.
    private static void ValidateRequest(CheckoutRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.IdempotencyKey) || request.IdempotencyKey.Length > 100)
        {
            throw new BusinessException("IdempotencyKey es obligatorio y debe tener máximo 100 caracteres.");
        }

        if (request.Items is null || request.Items.Count == 0)
        {
            throw new BusinessException("La venta debe contener al menos un producto.");
        }

        if (request.Items.Any(x => x.ProductId <= 0 || x.Quantity <= 0))
        {
            throw new BusinessException("Cada producto y cantidad deben ser válidos.");
        }
    }
}
