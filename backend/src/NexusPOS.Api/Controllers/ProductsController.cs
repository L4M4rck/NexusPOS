using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusPOS.Application.Catalog;
using NexusPOS.Application.Common;

namespace NexusPOS.Api.Controllers;
// Gestiona la consulta pública del catálogo y las operaciones de mantenimiento
// de productos reservadas al administrador.
[ApiController]
[Authorize]
[Route("api/products")]
public sealed class ProductsController(ICatalogService catalogService) : ControllerBase
{
    // GET /api/products: devuelve productos paginados, filtrados y ordenados.
    [Authorize(Roles = "Guest,Customer,Admin")]
    [HttpGet]
    public async Task<ActionResult<PagedResponse<ProductResponse>>> GetAll(
        [FromQuery] ProductQuery query,
        CancellationToken cancellationToken)
    {
        // Aunque el cliente envíe includeInactive=true, solo el rol Admin puede hacerlo efectivo.
        var effectiveQuery = query with { IncludeInactive = this.IsAdmin() && query.IncludeInactive };
        return Ok(await catalogService.GetProductsAsync(effectiveQuery, cancellationToken));
    }
    // GET /api/products/{id}: obtiene un producto; los inactivos solo son visibles para Admin.
    [Authorize(Roles = "Guest,Customer,Admin")]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductResponse>> Get(int id, CancellationToken cancellationToken) =>
        Ok(await catalogService.GetProductAsync(id, this.IsAdmin(), cancellationToken));
    // POST /api/products: valida y crea un producto. Devuelve 201 y la ubicación del recurso.
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ProductResponse>> Create(CreateProductRequest request, CancellationToken cancellationToken)
    {
        var product = await catalogService.CreateProductAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = product.Id }, product);
    }
    // PUT /api/products/{id}: actualiza todos los datos editables del producto.
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductResponse>> Update(int id, UpdateProductRequest request, CancellationToken cancellationToken) =>
        Ok(await catalogService.UpdateProductAsync(id, request, cancellationToken));
    // PATCH /api/products/{id}/status: activa o desactiva el producto sin borrar su historial.
    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> SetStatus(int id, UpdateProductStatusRequest request, CancellationToken cancellationToken)
    {
        await catalogService.SetProductStatusAsync(id, request.IsActive, cancellationToken);
        return NoContent();
    }
}
