namespace NexusPOS.Domain.Entities;
// Línea de detalle que relaciona una venta con un producto y conserva los valores históricos.
public sealed class SaleItem
{
    public long Id { get; set; }
    public long SaleId { get; set; }
    public int ProductId { get; set; }

    // El snapshot evita que renombrar el producto altere una factura emitida anteriormente.
    public required string ProductNameSnapshot { get; set; }
    public int Quantity { get; set; }

    // UnitPrice es el precio aprobado durante checkout, no el precio actual del catálogo.
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
    public Sale Sale { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
