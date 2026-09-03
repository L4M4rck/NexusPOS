using NexusPOS.Application.Checkout;
using NexusPOS.Application.Common;

namespace NexusPOS.Application.Invoices;

public sealed record InvoiceResponse(
    long Id,
    string Number,
    string CustomerName,
    string CustomerDocument,
    DateTime IssuedAt,
    decimal Subtotal,
    decimal Tax,
    decimal Discount,
    decimal Total,
    IReadOnlyList<SaleItemResponse> Items);

public sealed record InvoiceQuery(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    string Sort = "date_desc");

public interface IInvoiceService
{
    Task<IReadOnlyList<InvoiceResponse>> GetInvoicesAsync(int userId, bool isAdmin, CancellationToken cancellationToken);
    Task<PagedResponse<InvoiceResponse>> GetMovementsAsync(int userId, bool isAdmin, InvoiceQuery request, CancellationToken cancellationToken);
    Task<InvoiceResponse> GetInvoiceAsync(long id, int userId, bool isAdmin, CancellationToken cancellationToken);
}
