using Microsoft.EntityFrameworkCore;
using NexusPOS.Application.Catalog;
using NexusPOS.Application.Common;
using NexusPOS.Domain.Entities;
using NexusPOS.Domain.Exceptions;
using NexusPOS.Infrastructure.Persistence;

namespace NexusPOS.Infrastructure.Catalog;

internal sealed class CatalogService(NexusPosDbContext dbContext) : ICatalogService
{
    private const int MaxPageSize = 50;

    public async Task<PagedResponse<ProductResponse>> GetProductsAsync(ProductQuery query, CancellationToken cancellationToken)
    {
        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);
        var products = dbContext.Products.AsNoTracking().Include(x => x.Category).AsQueryable();

        if (!query.IncludeInactive)
        {
            products = products.Where(x => x.IsActive && x.Category.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            products = products.Where(x => x.Name.Contains(search) || x.Sku.Contains(search) || x.Description.Contains(search));
        }

        if (query.CategoryId.HasValue)
        {
            products = products.Where(x => x.CategoryId == query.CategoryId.Value);
        }

        products = query.Sort.ToLowerInvariant() switch
        {
            "price_asc" => products.OrderBy(x => x.Price),
            "price_desc" => products.OrderByDescending(x => x.Price),
            "newest" => products.OrderByDescending(x => x.CreatedAt),
            "name_desc" => products.OrderByDescending(x => x.Name),
            _ => products.OrderBy(x => x.Name)
        };

        var totalItems = await products.CountAsync(cancellationToken);
        var items = await products.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new ProductResponse(
                x.Id, x.Sku, x.Name, x.Description, x.Price, x.Stock,
                x.CategoryId, x.Category.Name, x.ImageUrl, x.IsActive))
            .ToListAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        return new PagedResponse<ProductResponse>(items, page, pageSize, totalItems, totalPages);
    }

    public async Task<ProductResponse> GetProductAsync(int id, bool includeInactive, CancellationToken cancellationToken)
    {
        var product = await dbContext.Products.AsNoTracking().Include(x => x.Category)
            .SingleOrDefaultAsync(x => x.Id == id && (includeInactive || x.IsActive), cancellationToken)
            ?? throw new NotFoundException("Producto no encontrado.");
        return ToResponse(product);
    }

    public async Task<ProductResponse> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken)
    {
        await ValidateProductAsync(request.Sku, request.Name, request.Price, request.Stock, request.CategoryId, null, cancellationToken);
        var product = new Product
        {
            Sku = request.Sku.Trim().ToUpperInvariant(),
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            Price = request.Price,
            Stock = request.Stock,
            CategoryId = request.CategoryId,
            ImageUrl = request.ImageUrl?.Trim()
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync(cancellationToken);
        await dbContext.Entry(product).Reference(x => x.Category).LoadAsync(cancellationToken);
        return ToResponse(product);
    }

    public async Task<ProductResponse> UpdateProductAsync(int id, UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var product = await dbContext.Products.Include(x => x.Category).SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Producto no encontrado.");
        await ValidateProductAsync(request.Sku, request.Name, request.Price, request.Stock, request.CategoryId, id, cancellationToken);
        product.Sku = request.Sku.Trim().ToUpperInvariant();
        product.Name = request.Name.Trim();
        product.Description = request.Description.Trim();
        product.Price = request.Price;
        product.Stock = request.Stock;
        product.CategoryId = request.CategoryId;
        product.ImageUrl = request.ImageUrl?.Trim();
        product.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        await dbContext.Entry(product).Reference(x => x.Category).LoadAsync(cancellationToken);
        return ToResponse(product);
    }

    public async Task SetProductStatusAsync(int id, bool isActive, CancellationToken cancellationToken)
    {
        var product = await dbContext.Products.SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Producto no encontrado.");
        product.IsActive = isActive;
        product.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CategoryResponse>> GetCategoriesAsync(bool includeInactive, CancellationToken cancellationToken) =>
        await dbContext.Categories.AsNoTracking()
            .Where(x => includeInactive || x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new CategoryResponse(x.Id, x.Name, x.Description, x.ImageUrl, x.IsActive))
            .ToListAsync(cancellationToken);

    public async Task<CategoryResponse> CreateCategoryAsync(SaveCategoryRequest request, CancellationToken cancellationToken)
    {
        ValidateCategory(request.Name);
        var normalized = request.Name.Trim();
        if (await dbContext.Categories.AnyAsync(x => x.Name == normalized, cancellationToken))
        {
            throw new ConflictException("Ya existe una categoría con ese nombre.");
        }

        var category = new Category
        {
            Name = normalized,
            Description = request.Description?.Trim(),
            ImageUrl = NormalizeOptional(request.ImageUrl),
            IsActive = request.IsActive
        };
        dbContext.Categories.Add(category);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new CategoryResponse(category.Id, category.Name, category.Description, category.ImageUrl, category.IsActive);
    }

    public async Task<CategoryResponse> UpdateCategoryAsync(int id, SaveCategoryRequest request, CancellationToken cancellationToken)
    {
        ValidateCategory(request.Name);
        var category = await dbContext.Categories.SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Categoría no encontrada.");
        var normalized = request.Name.Trim();
        if (await dbContext.Categories.AnyAsync(x => x.Id != id && x.Name == normalized, cancellationToken))
        {
            throw new ConflictException("Ya existe una categoría con ese nombre.");
        }

        category.Name = normalized;
        category.Description = request.Description?.Trim();
        category.ImageUrl = NormalizeOptional(request.ImageUrl);
        category.IsActive = request.IsActive;
        await dbContext.SaveChangesAsync(cancellationToken);
        return new CategoryResponse(category.Id, category.Name, category.Description, category.ImageUrl, category.IsActive);
    }

    private async Task ValidateProductAsync(string sku, string name, decimal price, int stock, int categoryId, int? currentId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(sku) || string.IsNullOrWhiteSpace(name))
        {
            throw new BusinessException("SKU y nombre son obligatorios.");
        }

        if (price <= 0 || stock < 0)
        {
            throw new BusinessException("El precio debe ser mayor que cero y el stock no puede ser negativo.");
        }

        var normalizedSku = sku.Trim().ToUpperInvariant();
        if (await dbContext.Products.AnyAsync(x => x.Sku == normalizedSku && x.Id != currentId, cancellationToken))
        {
            throw new ConflictException("Ya existe un producto con ese SKU.", "duplicate-sku");
        }

        if (!await dbContext.Categories.AnyAsync(x => x.Id == categoryId && x.IsActive, cancellationToken))
        {
            throw new BusinessException("La categoría seleccionada no existe o está inactiva.");
        }
    }

    private static void ValidateCategory(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new BusinessException("El nombre de la categoría es obligatorio.");
        }
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static ProductResponse ToResponse(Product product) => new(
        product.Id, product.Sku, product.Name, product.Description, product.Price, product.Stock,
        product.CategoryId, product.Category.Name, product.ImageUrl, product.IsActive);
}
