namespace NexusPOS.Domain.Entities;

public sealed class Customer
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public required string DocumentNumber { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public User User { get; set; } = null!;
    public ICollection<Sale> Sales { get; set; } = [];
}
