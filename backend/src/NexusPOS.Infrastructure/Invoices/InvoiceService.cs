using Microsoft.EntityFrameworkCore;
using NexusPOS.Application.Invoices;
using NexusPOS.Domain.Exceptions;
using NexusPOS.Infrastructure.Persistence;
using NexusPOS.Infrastructure.Sales;

namespace NexusPOS.Infrastructure.Invoices;

internal sealed class InvoiceService(NexusPosDbContext dbContext) : IInvoiceService
{
    public async Task<IReadOnlyList<InvoiceResponse>> GetInvoicesAsync(int userId, bool isAdmin, CancellationToken cancellationToken)
    {
        var query = dbContext.Invoices.AsNoTracking().Include(x => x.Sale).ThenInclude(x => x.Items).AsQueryable();
        if (!isAdmin)
        {
            query = query.Where(x => x.Sale.Customer.UserId == userId);
        }

        var invoices = await query.OrderByDescending(x => x.IssuedAt).Take(200).ToListAsync(cancellationToken);
        return invoices.Select(x => x.ToInvoiceResponse()).ToArray();
    }

    public async Task<InvoiceResponse> GetInvoiceAsync(long id, int userId, bool isAdmin, CancellationToken cancellationToken)
    {
        var invoice = await dbContext.Invoices.AsNoTracking().Include(x => x.Sale).ThenInclude(x => x.Items)
            .SingleOrDefaultAsync(x => x.Id == id && (isAdmin || x.Sale.Customer.UserId == userId), cancellationToken)
            ?? throw new NotFoundException("Factura no encontrada.");
        return invoice.ToInvoiceResponse();
    }
}
