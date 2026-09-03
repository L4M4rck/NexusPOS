using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusPOS.Application.Checkout;

namespace NexusPOS.Api.Controllers;

[ApiController]
[Authorize(Roles = "Customer,Admin")]
[Route("api/sales")]
public sealed class SalesController(ISalesService salesService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SaleResponse>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await salesService.GetSalesAsync(this.CurrentUserId(), this.IsAdmin(), cancellationToken));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<SaleResponse>> Get(long id, CancellationToken cancellationToken) =>
        Ok(await salesService.GetSaleAsync(id, this.CurrentUserId(), this.IsAdmin(), cancellationToken));
}
