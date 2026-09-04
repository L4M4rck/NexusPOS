namespace NexusPOS.Domain.Enums;

// Resultado conocido del procesamiento de un pago.
public enum PaymentStatus
{
    // El proveedor todavía no entrega una decisión definitiva.
    Pending,
    // El cobro fue aceptado.
    Approved,
    // El cobro fue rechazado.
    Rejected
}
