namespace NexusPOS.Domain.Entities;

public sealed class SaleItem
{
    public long Id { get; set; }
    public long SaleId { get; set; }
    public int ProductId { get; set; }
    public required string ProductNameSnapshot { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
    public Sale Sale { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
