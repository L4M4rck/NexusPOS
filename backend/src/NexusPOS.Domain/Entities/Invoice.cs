namespace NexusPOS.Domain.Entities;
// Documento histórico generado uno-a-uno para una venta completada.
public sealed class Invoice
{
    public long Id { get; set; }
    public long SaleId { get; set; }
    public required string Number { get; set; }

    // Los snapshots mantienen la identidad facturada aunque el cliente edite sus datos en el futuro.
    public required string CustomerNameSnapshot { get; set; }
    public required string CustomerDocumentSnapshot { get; set; }
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    public Sale Sale { get; set; } = null!;
}
