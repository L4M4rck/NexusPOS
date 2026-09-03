using NexusPOS.Application.Common;

namespace NexusPOS.Application.Catalog;

public sealed record ProductResponse(
    int Id,
    string Sku,
    string Name,
    string Description,
    decimal Price,
    int Stock,
    int CategoryId,
    string CategoryName,
    string? ImageUrl,
    bool IsActive);

public sealed record ProductQuery(
    int Page = 1,
    int PageSize = 12,
    string? Search = null,
    int? CategoryId = null,
    string Sort = "name_asc",
    bool IncludeInactive = false);

public sealed record CreateProductRequest(
    string Sku,
    string Name,
    string Description,
    decimal Price,
    int Stock,
    int CategoryId,
    string? ImageUrl);

public sealed record UpdateProductRequest(
    string Sku,
    string Name,
    string Description,
    decimal Price,
    int Stock,
    int CategoryId,
    string? ImageUrl);

public sealed record UpdateProductStatusRequest(bool IsActive);

public sealed record CategoryResponse(int Id, string Name, string? Description, string? ImageUrl, bool IsActive);
public sealed record SaveCategoryRequest(string Name, string? Description, string? ImageUrl, bool IsActive = true);

public interface ICatalogService
{
    Task<PagedResponse<ProductResponse>> GetProductsAsync(ProductQuery query, CancellationToken cancellationToken);
    Task<ProductResponse> GetProductAsync(int id, bool includeInactive, CancellationToken cancellationToken);
    Task<ProductResponse> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken);
    Task<ProductResponse> UpdateProductAsync(int id, UpdateProductRequest request, CancellationToken cancellationToken);
    Task SetProductStatusAsync(int id, bool isActive, CancellationToken cancellationToken);
    Task<IReadOnlyList<CategoryResponse>> GetCategoriesAsync(bool includeInactive, CancellationToken cancellationToken);
    Task<CategoryResponse> CreateCategoryAsync(SaveCategoryRequest request, CancellationToken cancellationToken);
    Task<CategoryResponse> UpdateCategoryAsync(int id, SaveCategoryRequest request, CancellationToken cancellationToken);
}
