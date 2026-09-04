using Microsoft.EntityFrameworkCore;
using NexusPOS.Application.Common;
using NexusPOS.Application.Invoices;
using NexusPOS.Domain.Exceptions;
using NexusPOS.Infrastructure.Persistence;
using NexusPOS.Infrastructure.Sales;

namespace NexusPOS.Infrastructure.Invoices;

// Consulta documentos facturados aplicando filtros de propiedad y administración.
internal sealed class InvoiceService(NexusPosDbContext dbContext) : IInvoiceService
{
    private const int MaxPageSize = 50;

    // Obtiene el historial reciente visible para la identidad actual.
    public async Task<IReadOnlyList<InvoiceResponse>> GetInvoicesAsync(int userId, bool isAdmin, CancellationToken cancellationToken)
    {
        var query = dbContext.Invoices.AsNoTracking().Include(x => x.Sale).ThenInclude(x => x.Items).AsQueryable();
        if (!isAdmin)
        {
            // El filtro de propiedad se ejecuta en SQL y evita cargar facturas ajenas en memoria.
            query = query.Where(x => x.Sale.Customer.UserId == userId);
        }

        var invoices = await query.OrderByDescending(x => x.IssuedAt).Take(200).ToListAsync(cancellationToken);
        return invoices.Select(x => x.ToInvoiceResponse()).ToArray();
    }

    // Resuelve búsqueda, ordenamiento y paginación del apartado Movimientos.
    public async Task<PagedResponse<InvoiceResponse>> GetMovementsAsync(
        int userId,
        bool isAdmin,
        InvoiceQuery request,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, MaxPageSize);
        var query = dbContext.Invoices.AsNoTracking().AsQueryable();
        if (!isAdmin)
        {
            query = query.Where(x => x.Sale.Customer.UserId == userId);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Number.ToLower().Contains(search) ||
                x.CustomerNameSnapshot.ToLower().Contains(search) ||
                x.CustomerDocumentSnapshot.ToLower().Contains(search));
        }

        // ThenBy(Id) estabiliza el orden cuando dos movimientos comparten fecha, número o total.
        query = request.Sort.ToLowerInvariant() switch
        {
            "date_asc" => query.OrderBy(x => x.IssuedAt).ThenBy(x => x.Id),
            "number_asc" => query.OrderBy(x => x.Number).ThenBy(x => x.Id),
            "number_desc" => query.OrderByDescending(x => x.Number).ThenByDescending(x => x.Id),
            "total_asc" => query.OrderBy(x => x.Sale.Total).ThenBy(x => x.Id),
            "total_desc" => query.OrderByDescending(x => x.Sale.Total).ThenByDescending(x => x.Id),
            _ => query.OrderByDescending(x => x.IssuedAt).ThenByDescending(x => x.Id)
        };

        var totalItems = await query.CountAsync(cancellationToken);
        var invoices = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(x => x.Sale)
            .ThenInclude(x => x.Items)
            // Divide la carga de relaciones para evitar multiplicación cartesiana de filas.
            .AsSplitQuery()
            .ToListAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        return new PagedResponse<InvoiceResponse>(
            invoices.Select(x => x.ToInvoiceResponse()).ToArray(),
            page,
            pageSize,
            totalItems,
            totalPages);
    }

    // Busca una factura por ID e integra autorización en la misma consulta.
    public async Task<InvoiceResponse> GetInvoiceAsync(long id, int userId, bool isAdmin, CancellationToken cancellationToken)
    {
        var invoice = await dbContext.Invoices.AsNoTracking().Include(x => x.Sale).ThenInclude(x => x.Items)
            .SingleOrDefaultAsync(x => x.Id == id && (isAdmin || x.Sale.Customer.UserId == userId), cancellationToken)
            ?? throw new NotFoundException("Factura no encontrada.");
        return invoice.ToInvoiceResponse();
    }
}
