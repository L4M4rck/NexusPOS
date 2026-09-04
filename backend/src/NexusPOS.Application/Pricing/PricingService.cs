using NexusPOS.Domain.Exceptions;

namespace NexusPOS.Application.Pricing;

// Modelos de entrada y salida de la regla monetaria. decimal evita errores binarios de float/double.
public sealed record PricingLine(int ProductId, string ProductName, int Quantity, decimal UnitPrice);
public sealed record PricedLine(int ProductId, string ProductName, int Quantity, decimal UnitPrice, decimal Subtotal);
public sealed record PricingResult(
    IReadOnlyList<PricedLine> Items,
    decimal Subtotal,
    decimal Tax,
    decimal Discount,
    decimal Total);

public interface IPricingService
{
    // Calcula líneas, subtotal, descuento, IVA y total.
    PricingResult Calculate(IEnumerable<PricingLine> lines);
}
// Regla de precios pura: no utiliza HTTP ni base de datos, por lo que puede probarse de forma aislada.
public sealed class PricingService(decimal taxRate) : IPricingService
{
    // Valida las líneas y calcula los valores monetarios con redondeo comercial a dos decimales.
    public PricingResult Calculate(IEnumerable<PricingLine> lines)
    {
        var source = lines.ToArray();
        if (source.Length == 0)
        {
            throw new BusinessException("La venta debe contener al menos un producto.");
        }

        if (source.Any(line => line.Quantity <= 0))
        {
            throw new BusinessException("Todas las cantidades deben ser mayores que cero.");
        }

        var pricedLines = source
            .Select(line => new PricedLine(
                line.ProductId,
                line.ProductName,
                line.Quantity,
                line.UnitPrice,
                decimal.Round(line.UnitPrice * line.Quantity, 2, MidpointRounding.AwayFromZero)))
            .ToArray();

        // El descuento está preparado como parte del contrato aunque la versión actual no aplica promociones.
        var subtotal = pricedLines.Sum(line => line.Subtotal);
        var discount = 0m;
        var tax = decimal.Round((subtotal - discount) * taxRate, 2, MidpointRounding.AwayFromZero);
        return new PricingResult(pricedLines, subtotal, tax, discount, subtotal - discount + tax);
    }
}
