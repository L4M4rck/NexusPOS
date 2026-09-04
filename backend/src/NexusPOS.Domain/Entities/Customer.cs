namespace NexusPOS.Domain.Entities;
// Perfil comercial asociado uno-a-uno a un usuario con rol Customer.
public sealed class Customer
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public required string DocumentNumber { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Propiedades de navegación utilizadas por EF Core para recorrer relaciones sin exponer claves manualmente.
    public User User { get; set; } = null!;
    public ICollection<Sale> Sales { get; set; } = [];
}
