using NexusPOS.Application.Checkout;

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

public interface IInvoiceService
{
    Task<IReadOnlyList<InvoiceResponse>> GetInvoicesAsync(int userId, bool isAdmin, CancellationToken cancellationToken);
    Task<InvoiceResponse> GetInvoiceAsync(long id, int userId, bool isAdmin, CancellationToken cancellationToken);
}
