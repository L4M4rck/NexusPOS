namespace NexusPOS.Application.Admin;

// DTO pequeños que representan cada bloque visual del dashboard administrativo.
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
// Define las consultas administrativas sin acoplar Application a Entity Framework.
public interface IAdminService
{
    // Construye indicadores y gráficas del periodo weekly, monthly o yearly.
    Task<DashboardResponse> GetDashboardAsync(string period, CancellationToken cancellationToken);

    // Lista los perfiles de clientes registrados.
    Task<IReadOnlyList<CustomerResponse>> GetCustomersAsync(CancellationToken cancellationToken);

    // Obtiene un cliente por su identificador.
    Task<CustomerResponse> GetCustomerAsync(int id, CancellationToken cancellationToken);
}
