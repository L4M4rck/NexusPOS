using NexusPOS.Domain.Enums;

namespace NexusPOS.Domain.Entities;

public sealed class Payment
{
    public long Id { get; set; }
    public long SaleId { get; set; }
    public required string Provider { get; set; }
    public required string ProviderReference { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "COP";
    public PaymentStatus Status { get; set; }
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    public Sale Sale { get; set; } = null!;
}
