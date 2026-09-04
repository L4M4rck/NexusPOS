using NexusPOS.Domain.Enums;

namespace NexusPOS.Domain.Entities;
// Cabecera de una operación comercial completada por un cliente.
// Agrupa totales, detalles, pago y factura.
public sealed class Sale
{
    public long Id { get; set; }

    // La combinación CustomerId + IdempotencyKey es única y evita ventas duplicadas por reintentos.
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

    // Una venta contiene muchas líneas, pero un único pago y una única factura en el modelo actual.
    public ICollection<SaleItem> Items { get; set; } = [];
    public Payment Payment { get; set; } = null!;
    public Invoice Invoice { get; set; } = null!;
}
