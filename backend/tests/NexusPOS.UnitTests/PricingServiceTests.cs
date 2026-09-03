using FluentAssertions;
using NexusPOS.Application.Pricing;
using NexusPOS.Domain.Exceptions;
using Xunit;

namespace NexusPOS.UnitTests;

public sealed class PricingServiceTests
{
    private readonly PricingService _service = new(0.19m);

    [Fact]
    public void Calculate_WithValidLines_CalculatesSubtotalTaxAndTotal()
    {
        var result = _service.Calculate([
            new PricingLine(1, "Mouse", 2, 100_000m),
            new PricingLine(2, "Teclado", 1, 250_000m)
        ]);

        result.Subtotal.Should().Be(450_000m);
        result.Tax.Should().Be(85_500m);
        result.Total.Should().Be(535_500m);
    }

    [Fact]
    public void Calculate_UsesSuppliedServerPriceForEachLine()
    {
        var result = _service.Calculate([new PricingLine(10, "Producto", 3, 249_900m)]);

        result.Items.Single().UnitPrice.Should().Be(249_900m);
        result.Items.Single().Subtotal.Should().Be(749_700m);
    }

    [Fact]
    public void Calculate_WithEmptyItems_ShouldFail()
    {
        var action = () => _service.Calculate([]);

        action.Should().Throw<BusinessException>().WithMessage("*al menos un producto*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Calculate_WithInvalidQuantity_ShouldFail(int quantity)
    {
        var action = () => _service.Calculate([new PricingLine(1, "Mouse", quantity, 100m)]);

        action.Should().Throw<BusinessException>().WithMessage("*mayores que cero*");
    }
}
