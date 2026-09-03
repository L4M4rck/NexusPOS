using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusPOS.Application.Catalog;
using NexusPOS.Application.Common;

namespace NexusPOS.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/products")]
public sealed class ProductsController(ICatalogService catalogService) : ControllerBase
{
    [Authorize(Roles = "Guest,Customer,Admin")]
    [HttpGet]
    public async Task<ActionResult<PagedResponse<ProductResponse>>> GetAll(
        [FromQuery] ProductQuery query,
        CancellationToken cancellationToken)
    {
        var effectiveQuery = query with { IncludeInactive = this.IsAdmin() && query.IncludeInactive };
        return Ok(await catalogService.GetProductsAsync(effectiveQuery, cancellationToken));
    }

    [Authorize(Roles = "Guest,Customer,Admin")]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductResponse>> Get(int id, CancellationToken cancellationToken) =>
        Ok(await catalogService.GetProductAsync(id, this.IsAdmin(), cancellationToken));

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ProductResponse>> Create(CreateProductRequest request, CancellationToken cancellationToken)
    {
        var product = await catalogService.CreateProductAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = product.Id }, product);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductResponse>> Update(int id, UpdateProductRequest request, CancellationToken cancellationToken) =>
        Ok(await catalogService.UpdateProductAsync(id, request, cancellationToken));

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> SetStatus(int id, UpdateProductStatusRequest request, CancellationToken cancellationToken)
    {
        await catalogService.SetProductStatusAsync(id, request.IsActive, cancellationToken);
        return NoContent();
    }
}
