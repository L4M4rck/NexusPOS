using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusPOS.Application.Catalog;

namespace NexusPOS.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/categories")]
public sealed class CategoriesController(ICatalogService catalogService) : ControllerBase
{
    [Authorize(Roles = "Guest,Customer,Admin")]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoryResponse>>> GetAll([FromQuery] bool includeInactive, CancellationToken cancellationToken) =>
        Ok(await catalogService.GetCategoriesAsync(this.IsAdmin() && includeInactive, cancellationToken));

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<CategoryResponse>> Create(SaveCategoryRequest request, CancellationToken cancellationToken)
    {
        var category = await catalogService.CreateCategoryAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, category);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<CategoryResponse>> Update(int id, SaveCategoryRequest request, CancellationToken cancellationToken) =>
        Ok(await catalogService.UpdateCategoryAsync(id, request, cancellationToken));
}
