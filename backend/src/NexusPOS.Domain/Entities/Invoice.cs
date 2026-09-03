namespace NexusPOS.Domain.Entities;

public sealed class Invoice
{
    public long Id { get; set; }
    public long SaleId { get; set; }
    public required string Number { get; set; }
    public required string CustomerNameSnapshot { get; set; }
    public required string CustomerDocumentSnapshot { get; set; }
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    public Sale Sale { get; set; } = null!;
}
