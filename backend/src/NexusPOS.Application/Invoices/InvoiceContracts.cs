using NexusPOS.Application.Checkout;
using NexusPOS.Application.Common;

namespace NexusPOS.Application.Invoices;

// DTO histórico: presenta snapshots del cliente y líneas de venta, no valores actuales editables.
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

// Parámetros de búsqueda, ordenamiento y paginación de la pantalla Movimientos.
public sealed record InvoiceQuery(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    string Sort = "date_desc");

// Define consultas de factura con control de propiedad.
public interface IInvoiceService
{
    // Lista el historial visible para el cliente o todas las facturas para Admin.
    Task<IReadOnlyList<InvoiceResponse>> GetInvoicesAsync(int userId, bool isAdmin, CancellationToken cancellationToken);

    // Consulta Movimientos en el servidor aplicando filtros, orden y paginación.
    Task<PagedResponse<InvoiceResponse>> GetMovementsAsync(int userId, bool isAdmin, InvoiceQuery request, CancellationToken cancellationToken);

    // Obtiene una factura específica después de verificar su propiedad.
    Task<InvoiceResponse> GetInvoiceAsync(long id, int userId, bool isAdmin, CancellationToken cancellationToken);
}
