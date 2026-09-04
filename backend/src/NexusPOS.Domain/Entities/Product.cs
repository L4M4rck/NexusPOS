namespace NexusPOS.Domain.Entities;
// Artículo vendible del catálogo con precio vigente, existencias y referencia SKU única.
public sealed class Product
{
    public int Id { get; set; }
    public required string Sku { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }

    // Price y Stock tienen restricciones adicionales en MySQL: precio positivo y stock no negativo.
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public int CategoryId { get; set; }
    public string? ImageUrl { get; set; }

    // Los productos se desactivan en vez de eliminarse para conservar las ventas relacionadas.
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public Category Category { get; set; } = null!;
    public ICollection<SaleItem> SaleItems { get; set; } = [];
}
