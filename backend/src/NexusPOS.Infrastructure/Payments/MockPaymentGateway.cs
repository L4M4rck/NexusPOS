using NexusPOS.Application.Checkout;

namespace NexusPOS.Infrastructure.Payments;
// Simulador determinista de pagos para desarrollo: mock-rejected rechaza y
// cualquier otro método aprueba. No contacta bancos ni almacena tarjetas.
internal sealed class MockPaymentGateway : IPaymentGateway
{
    // Genera una referencia única y devuelve el resultado simulado.
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
