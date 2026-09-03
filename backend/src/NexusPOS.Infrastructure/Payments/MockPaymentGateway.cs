using NexusPOS.Application.Checkout;

namespace NexusPOS.Infrastructure.Payments;

internal sealed class MockPaymentGateway : IPaymentGateway
{
    public Task<PaymentResult> ProcessAsync(PaymentRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var approved = !request.PaymentMethod.Equals("mock-rejected", StringComparison.OrdinalIgnoreCase);
        var reference = $"MOCK-{Guid.NewGuid():N}";
        return Task.FromResult(approved
            ? new PaymentResult(true, reference)
            : new PaymentResult(false, reference, "Pago rechazado por el simulador."));
    }
}
