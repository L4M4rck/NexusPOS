namespace NexusPOS.Application.Checkout;

public sealed record CheckoutItemRequest(int ProductId, int Quantity);

public sealed record CheckoutRequest(
    string IdempotencyKey,
    IReadOnlyList<CheckoutItemRequest> Items,
    string PaymentMethod = "mock-approved");

public sealed record SaleItemResponse(
    int ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal Subtotal);

public sealed record SaleResponse(
    long Id,
    string InvoiceNumber,
    long InvoiceId,
    decimal Subtotal,
    decimal Tax,
    decimal Discount,
    decimal Total,
    string Status,
    DateTime CreatedAt,
    IReadOnlyList<SaleItemResponse> Items);

public sealed record PaymentRequest(decimal Amount, string Currency, string PaymentMethod, string IdempotencyKey);
public sealed record PaymentResult(bool IsApproved, string ProviderReference, string? DeclineReason = null);

public interface IPaymentGateway
{
    Task<PaymentResult> ProcessAsync(PaymentRequest request, CancellationToken cancellationToken);
}

public interface ICheckoutService
{
    Task<SaleResponse> CheckoutAsync(int userId, CheckoutRequest request, CancellationToken cancellationToken);
}

public interface ISalesService
{
    Task<IReadOnlyList<SaleResponse>> GetSalesAsync(int userId, bool isAdmin, CancellationToken cancellationToken);
    Task<SaleResponse> GetSaleAsync(long id, int userId, bool isAdmin, CancellationToken cancellationToken);
}
