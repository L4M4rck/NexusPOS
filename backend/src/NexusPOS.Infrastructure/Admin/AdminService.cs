using Microsoft.EntityFrameworkCore;
using NexusPOS.Application.Admin;
using NexusPOS.Domain.Enums;
using NexusPOS.Domain.Exceptions;
using NexusPOS.Infrastructure.Persistence;

namespace NexusPOS.Infrastructure.Admin;

internal sealed class AdminService(NexusPosDbContext dbContext, int lowStockThreshold) : IAdminService
{
    public async Task<DashboardResponse> GetDashboardAsync(string period, CancellationToken cancellationToken)
    {
        var normalizedPeriod = period.ToLowerInvariant();
        var now = DateTime.UtcNow;
        var from = normalizedPeriod switch
        {
            "weekly" => now.Date.AddDays(-6),
            "monthly" => now.Date.AddDays(-29),
            "yearly" => new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            _ => throw new BusinessException("El periodo debe ser weekly, monthly o yearly.")
        };

        var sales = await dbContext.Sales.AsNoTracking()
            .Include(x => x.Items)
            .Include(x => x.Customer).ThenInclude(x => x.User)
            .Where(x => x.Status == SaleStatus.Completed && x.CreatedAt >= from)
            .ToListAsync(cancellationToken);

        var series = BuildSeries(sales, normalizedPeriod, from, now);
        var topProducts = sales.SelectMany(x => x.Items)
            .GroupBy(x => x.ProductNameSnapshot)
            .Select(group => new TopProductResponse(group.Key, group.Sum(x => x.Quantity), group.Sum(x => x.Subtotal)))
            .OrderByDescending(x => x.Units)
            .Take(5)
            .ToArray();
        var lowStock = await dbContext.Products.AsNoTracking()
            .Where(x => x.IsActive && x.Stock <= lowStockThreshold)
            .OrderBy(x => x.Stock)
            .Take(10)
            .Select(x => new LowStockProductResponse(x.Id, x.Name, x.Stock))
            .ToListAsync(cancellationToken);
        var recent = sales.OrderByDescending(x => x.CreatedAt).Take(5)
            .Select(x => new RecentSaleResponse(x.Id, x.InvoiceNumber, $"{x.Customer.User.FirstName} {x.Customer.User.LastName}", x.Total, x.CreatedAt))
            .ToArray();
        var revenue = sales.Sum(x => x.Total);
        return new DashboardResponse(
            normalizedPeriod,
            revenue,
            sales.Count,
            sales.Select(x => x.CustomerId).Distinct().Count(),
            sales.SelectMany(x => x.Items).Sum(x => x.Quantity),
            sales.Count == 0 ? 0 : decimal.Round(revenue / sales.Count, 2),
            series,
            topProducts,
            lowStock,
            recent);
    }

    public async Task<IReadOnlyList<CustomerResponse>> GetCustomersAsync(CancellationToken cancellationToken)
    {
        var customers = await dbContext.Customers.AsNoTracking().Include(x => x.User)
            .OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);
        return customers.Select(MapCustomer).ToArray();
    }

    public async Task<CustomerResponse> GetCustomerAsync(int id, CancellationToken cancellationToken)
    {
        var customer = await dbContext.Customers.AsNoTracking().Include(x => x.User)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Cliente no encontrado.");
        return MapCustomer(customer);
    }

    private static IReadOnlyList<DashboardSeriesPoint> BuildSeries(
        IReadOnlyList<Domain.Entities.Sale> sales,
        string period,
        DateTime from,
        DateTime now)
    {
        if (period == "yearly")
        {
            return Enumerable.Range(1, now.Month)
                .Select(month =>
                {
                    var matching = sales.Where(x => x.CreatedAt.Month == month);
                    return new DashboardSeriesPoint(new DateTime(now.Year, month, 1).ToString("MMM"), matching.Count(), matching.Sum(x => x.Total));
                }).ToArray();
        }

        return Enumerable.Range(0, (now.Date - from.Date).Days + 1)
            .Select(offset =>
            {
                var day = from.Date.AddDays(offset);
                var matching = sales.Where(x => x.CreatedAt.Date == day);
                return new DashboardSeriesPoint(day.ToString(period == "weekly" ? "ddd" : "dd MMM"), matching.Count(), matching.Sum(x => x.Total));
            }).ToArray();
    }

    private static CustomerResponse MapCustomer(Domain.Entities.Customer customer) => new(
        customer.Id,
        customer.User.FirstName,
        customer.User.LastName,
        customer.User.Email,
        customer.DocumentNumber,
        customer.Phone,
        customer.Address,
        customer.User.IsActive,
        customer.CreatedAt);
}
