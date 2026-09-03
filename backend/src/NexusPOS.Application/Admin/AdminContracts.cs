namespace NexusPOS.Application.Admin;

public sealed record DashboardSeriesPoint(string Label, int Sales, decimal Revenue);
public sealed record TopProductResponse(string Name, int Units, decimal Revenue);
public sealed record LowStockProductResponse(int Id, string Name, int Stock);
public sealed record RecentSaleResponse(long Id, string InvoiceNumber, string Customer, decimal Total, DateTime CreatedAt);

public sealed record DashboardResponse(
    string Period,
    decimal Revenue,
    int SalesCount,
    int CustomersCount,
    int UnitsSold,
    decimal AverageTicket,
    IReadOnlyList<DashboardSeriesPoint> Series,
    IReadOnlyList<TopProductResponse> TopProducts,
    IReadOnlyList<LowStockProductResponse> LowStockProducts,
    IReadOnlyList<RecentSaleResponse> RecentSales);

public sealed record CustomerResponse(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string DocumentNumber,
    string? Phone,
    string? Address,
    bool IsActive,
    DateTime CreatedAt);

public interface IAdminService
{
    Task<DashboardResponse> GetDashboardAsync(string period, CancellationToken cancellationToken);
    Task<IReadOnlyList<CustomerResponse>> GetCustomersAsync(CancellationToken cancellationToken);
    Task<CustomerResponse> GetCustomerAsync(int id, CancellationToken cancellationToken);
}
