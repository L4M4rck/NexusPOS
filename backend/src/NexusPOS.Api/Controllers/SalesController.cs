using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusPOS.Application.Checkout;

namespace NexusPOS.Api.Controllers;
// Expone el historial comercial de ventas con control de propiedad por usuario.
[ApiController]
[Authorize(Roles = "Customer,Admin")]
[Route("api/sales")]
public sealed class SalesController(ISalesService salesService) : ControllerBase
{
    // GET /api/sales: lista ventas propias para Customer o todas para Admin.
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SaleResponse>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await salesService.GetSalesAsync(this.CurrentUserId(), this.IsAdmin(), cancellationToken));
    // GET /api/sales/{id}: obtiene una venta si pertenece al cliente actual o si es Admin.
    [HttpGet("{id:long}")]
    public async Task<ActionResult<SaleResponse>> Get(long id, CancellationToken cancellationToken) =>
        Ok(await salesService.GetSaleAsync(id, this.CurrentUserId(), this.IsAdmin(), cancellationToken));
}
