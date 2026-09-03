using NexusPOS.Domain.Enums;

namespace NexusPOS.Domain.Entities;

public sealed class Sale
{
    public long Id { get; set; }
    public required string IdempotencyKey { get; set; }
    public required string InvoiceNumber { get; set; }
    public int CustomerId { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
    public SaleStatus Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Customer Customer { get; set; } = null!;
    public ICollection<SaleItem> Items { get; set; } = [];
    public Payment Payment { get; set; } = null!;
    public Invoice Invoice { get; set; } = null!;
}
