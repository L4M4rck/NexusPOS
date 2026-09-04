using NexusPOS.Application.Common;

namespace NexusPOS.Application.Catalog;

// DTO devuelto al frontend; combina datos del producto con el nombre de su categoría.
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

// Filtros aceptados por GET /api/products. Los valores predeterminados hacen la consulta paginada.
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
// Contrato de lectura y mantenimiento del catálogo.
public interface ICatalogService
{
    // Busca, filtra, ordena y pagina productos.
    Task<PagedResponse<ProductResponse>> GetProductsAsync(ProductQuery query, CancellationToken cancellationToken);

    // Obtiene un producto y opcionalmente permite consultar inactivos.
    Task<ProductResponse> GetProductAsync(int id, bool includeInactive, CancellationToken cancellationToken);

    // Valida SKU, precio, stock y categoría antes de crear el producto.
    Task<ProductResponse> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken);

    // Valida y reemplaza la información editable de un producto.
    Task<ProductResponse> UpdateProductAsync(int id, UpdateProductRequest request, CancellationToken cancellationToken);

    // Activa o desactiva un producto sin eliminar sus ventas históricas.
    Task SetProductStatusAsync(int id, bool isActive, CancellationToken cancellationToken);

    // Lista categorías visibles o incluye inactivas para administración.
    Task<IReadOnlyList<CategoryResponse>> GetCategoriesAsync(bool includeInactive, CancellationToken cancellationToken);

    // Crea una categoría después de validar su nombre único.
    Task<CategoryResponse> CreateCategoryAsync(SaveCategoryRequest request, CancellationToken cancellationToken);

    // Actualiza una categoría existente.
    Task<CategoryResponse> UpdateCategoryAsync(int id, SaveCategoryRequest request, CancellationToken cancellationToken);
}
