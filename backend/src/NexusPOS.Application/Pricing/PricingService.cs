using NexusPOS.Domain.Exceptions;

namespace NexusPOS.Application.Pricing;

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
    PricingResult Calculate(IEnumerable<PricingLine> lines);
}

public sealed class PricingService(decimal taxRate) : IPricingService
{
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

        var subtotal = pricedLines.Sum(line => line.Subtotal);
        var discount = 0m;
        var tax = decimal.Round((subtotal - discount) * taxRate, 2, MidpointRounding.AwayFromZero);
        return new PricingResult(pricedLines, subtotal, tax, discount, subtotal - discount + tax);
    }
}
