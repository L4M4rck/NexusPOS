namespace NexusPOS.Domain.Entities;
// Agrupa productos del catálogo y aporta la imagen que se muestra antes de entrar a la categoría.
public sealed class Category
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }

    // La desactivación oculta la categoría sin borrar productos o información histórica.
    public bool IsActive { get; set; } = true;
    public ICollection<Product> Products { get; set; } = [];
}
