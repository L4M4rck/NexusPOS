using NexusPOS.Domain.Enums;

namespace NexusPOS.Domain.Entities;
// Evidencia del intento de cobro asociado uno-a-uno con una venta.
public sealed class Payment
{
    public long Id { get; set; }
    public long SaleId { get; set; }
    public required string Provider { get; set; }

    // Referencia única devuelta por el proveedor; permite rastrear el pago sin almacenar una tarjeta.
    public required string ProviderReference { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "COP";
    public PaymentStatus Status { get; set; }
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    public Sale Sale { get; set; } = null!;
}
