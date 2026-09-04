namespace NexusPOS.Application.Checkout;

// El navegador solo envía identificadores y cantidades; el backend recupera precios y stock reales.
public sealed record CheckoutItemRequest(int ProductId, int Quantity);

// IdempotencyKey identifica un intento de compra para impedir duplicados por reintentos o doble clic.
public sealed record CheckoutRequest(
    string IdempotencyKey,
    IReadOnlyList<CheckoutItemRequest> Items,
    string PaymentMethod = "mock-approved");

public sealed record SaleItemResponse(
    int ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal Subtotal);

public sealed record SaleResponse(
    long Id,
    string InvoiceNumber,
    long InvoiceId,
    decimal Subtotal,
    decimal Tax,
    decimal Discount,
    decimal Total,
    string Status,
    DateTime CreatedAt,
    IReadOnlyList<SaleItemResponse> Items);

public sealed record PaymentRequest(decimal Amount, string Currency, string PaymentMethod, string IdempotencyKey);
public sealed record PaymentResult(bool IsApproved, string ProviderReference, string? DeclineReason = null);
// Puerto que abstrae al proveedor de pagos. Infrastructure aporta la implementación simulada.
public interface IPaymentGateway
{
    // Procesa un intento de cobro y devuelve aprobación, referencia o motivo de rechazo.
    Task<PaymentResult> ProcessAsync(PaymentRequest request, CancellationToken cancellationToken);
}

// Orquesta el caso de uso transaccional de compra.
public interface ICheckoutService
{
    // Crea una venta para el cliente asociado al usuario autenticado.
    Task<SaleResponse> CheckoutAsync(int userId, CheckoutRequest request, CancellationToken cancellationToken);
}

// Consulta ventas aplicando visibilidad de cliente o administrador.
public interface ISalesService
{
    // Lista las ventas que el usuario tiene permiso de observar.
    Task<IReadOnlyList<SaleResponse>> GetSalesAsync(int userId, bool isAdmin, CancellationToken cancellationToken);

    // Obtiene una venta por ID con control de propiedad.
    Task<SaleResponse> GetSaleAsync(long id, int userId, bool isAdmin, CancellationToken cancellationToken);
}
