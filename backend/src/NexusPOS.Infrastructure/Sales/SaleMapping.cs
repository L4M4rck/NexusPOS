using NexusPOS.Application.Checkout;
using NexusPOS.Application.Invoices;
using NexusPOS.Domain.Entities;

namespace NexusPOS.Infrastructure.Sales;
// Centraliza conversiones de entidades EF a DTO para no exponer el modelo de persistencia.
internal static class SaleMapping
{
    // Convierte una venta y sus líneas al contrato de respuesta.
    public static SaleResponse ToSaleResponse(this Sale sale) => new(
        sale.Id,
        sale.InvoiceNumber,
        sale.Invoice?.Id ?? 0,
        sale.Subtotal,
        sale.Tax,
        sale.Discount,
        sale.Total,
        sale.Status.ToString(),
        sale.CreatedAt,
        sale.Items.Select(ToItemResponse).ToArray());

    // Compone la factura con snapshots y totales provenientes de su venta.
    public static InvoiceResponse ToInvoiceResponse(this Invoice invoice) => new(
        invoice.Id,
        invoice.Number,
        invoice.CustomerNameSnapshot,
        invoice.CustomerDocumentSnapshot,
        invoice.IssuedAt,
        invoice.Sale.Subtotal,
        invoice.Sale.Tax,
        invoice.Sale.Discount,
        invoice.Sale.Total,
        invoice.Sale.Items.Select(ToItemResponse).ToArray());

    // Convierte una línea histórica al formato compartido por venta y factura.
    private static SaleItemResponse ToItemResponse(SaleItem item) => new(
        item.ProductId, item.ProductNameSnapshot, item.Quantity, item.UnitPrice, item.Subtotal);
}
