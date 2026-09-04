using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusPOS.Application.Common;
using NexusPOS.Application.Invoices;

namespace NexusPOS.Api.Controllers;
// Consulta facturas. Los clientes solo reciben documentos propios, mientras
// que un administrador puede consultar la operación completa.
[ApiController]
[Authorize(Roles = "Customer,Admin")]
[Route("api/invoices")]
public sealed class InvoicesController(IInvoiceService invoiceService) : ControllerBase
{
    // GET /api/invoices: obtiene el historial de facturas visible para el usuario actual.
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<InvoiceResponse>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await invoiceService.GetInvoicesAsync(this.CurrentUserId(), this.IsAdmin(), cancellationToken));
    // GET /api/invoices/movements: lista facturas con búsqueda, ordenamiento y paginación.
    [HttpGet("movements")]
    public async Task<ActionResult<PagedResponse<InvoiceResponse>>> GetMovements(
        [FromQuery] InvoiceQuery query,
        CancellationToken cancellationToken) =>
        Ok(await invoiceService.GetMovementsAsync(this.CurrentUserId(), this.IsAdmin(), query, cancellationToken));
    // GET /api/invoices/{id}: obtiene el detalle de una factura validando propiedad o rol Admin.
    [HttpGet("{id:long}")]
    public async Task<ActionResult<InvoiceResponse>> Get(long id, CancellationToken cancellationToken) =>
        Ok(await invoiceService.GetInvoiceAsync(id, this.CurrentUserId(), this.IsAdmin(), cancellationToken));
}
