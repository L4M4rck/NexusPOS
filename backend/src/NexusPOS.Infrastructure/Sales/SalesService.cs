using Microsoft.EntityFrameworkCore;
using NexusPOS.Application.Checkout;
using NexusPOS.Domain.Exceptions;
using NexusPOS.Infrastructure.Persistence;

namespace NexusPOS.Infrastructure.Sales;

// Implementa consultas de ventas y aplica autorización por propiedad.
internal sealed class SalesService(NexusPosDbContext dbContext) : ISalesService
{
    // Lista hasta 200 ventas recientes visibles para el usuario.
    public async Task<IReadOnlyList<SaleResponse>> GetSalesAsync(int userId, bool isAdmin, CancellationToken cancellationToken)
    {
        var query = dbContext.Sales.AsNoTracking().Include(x => x.Items).Include(x => x.Invoice).AsQueryable();
        if (!isAdmin)
        {
            // La autorización se incorpora a la consulta para no cargar ventas ajenas.
            query = query.Where(x => x.Customer.UserId == userId);
        }

        var sales = await query.OrderByDescending(x => x.CreatedAt).Take(200).ToListAsync(cancellationToken);
        return sales.Select(x => x.ToSaleResponse()).ToArray();
    }

    // Obtiene una venta solo si el usuario es administrador o propietario.
    public async Task<SaleResponse> GetSaleAsync(long id, int userId, bool isAdmin, CancellationToken cancellationToken)
    {
        var sale = await dbContext.Sales.AsNoTracking().Include(x => x.Items).Include(x => x.Invoice)
            .SingleOrDefaultAsync(x => x.Id == id && (isAdmin || x.Customer.UserId == userId), cancellationToken)
            ?? throw new NotFoundException("Venta no encontrada.");
        return sale.ToSaleResponse();
    }
}
