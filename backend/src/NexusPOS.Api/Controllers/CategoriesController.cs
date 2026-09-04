using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusPOS.Application.Catalog;

namespace NexusPOS.Api.Controllers;
// Expone el catálogo de categorías y sus operaciones administrativas.
[ApiController]
[Authorize]
[Route("api/categories")]
public sealed class CategoriesController(ICatalogService catalogService) : ControllerBase
{
    // GET /api/categories: lista categorías visibles. Solo un administrador puede incluir inactivas.
    [Authorize(Roles = "Guest,Customer,Admin")]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoryResponse>>> GetAll([FromQuery] bool includeInactive, CancellationToken cancellationToken) =>
        Ok(await catalogService.GetCategoriesAsync(this.IsAdmin() && includeInactive, cancellationToken));
    // POST /api/categories: crea una categoría. Requiere rol Admin.
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<CategoryResponse>> Create(SaveCategoryRequest request, CancellationToken cancellationToken)
    {
        var category = await catalogService.CreateCategoryAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, category);
    }
    // PUT /api/categories/{id}: reemplaza los datos editables de una categoría existente.
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<CategoryResponse>> Update(int id, SaveCategoryRequest request, CancellationToken cancellationToken) =>
        Ok(await catalogService.UpdateCategoryAsync(id, request, cancellationToken));
}
