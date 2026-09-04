namespace NexusPOS.Domain.Enums;

// Estados posibles del ciclo comercial de una venta.
public enum SaleStatus
{
    // La operación todavía no ha concluido.
    Pending,
    // Pago, inventario y factura se confirmaron correctamente.
    Completed,
    // El proveedor rechazó o no pudo completar el pago.
    PaymentFailed,
    // La operación fue anulada.
    Cancelled
}
