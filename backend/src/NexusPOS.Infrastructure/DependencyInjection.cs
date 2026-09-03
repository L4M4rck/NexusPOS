using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NexusPOS.Application.Admin;
using NexusPOS.Application.Auth;
using NexusPOS.Application.Catalog;
using NexusPOS.Application.Checkout;
using NexusPOS.Application.Invoices;
using NexusPOS.Application.Pricing;
using NexusPOS.Domain.Entities;
using NexusPOS.Infrastructure.Admin;
using NexusPOS.Infrastructure.Auth;
using NexusPOS.Infrastructure.Catalog;
using NexusPOS.Infrastructure.Invoices;
using NexusPOS.Infrastructure.Payments;
using NexusPOS.Infrastructure.Persistence;
using NexusPOS.Infrastructure.Sales;

namespace NexusPOS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection no está configurada.");
        services.AddDbContext<NexusPosDbContext>(options => options.UseMySQL(connectionString));

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException("La configuración JWT no está disponible.");
        if (jwt.Secret.Length < 32)
        {
            throw new InvalidOperationException("JWT_SECRET debe contener al menos 32 caracteres.");
        }

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret)),
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });
        services.AddAuthorization();

        var taxRate = configuration.GetValue("Business:TaxRate", 0.19m);
        var lowStockThreshold = configuration.GetValue("Business:LowStockThreshold", 5);
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICatalogService, CatalogService>();
        services.AddScoped<IPricingService>(_ => new PricingService(taxRate));
        services.AddScoped<IPaymentGateway, MockPaymentGateway>();
        services.AddScoped<ICheckoutService, CheckoutService>();
        services.AddScoped<ISalesService, SalesService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<IAdminService>(provider => new AdminService(provider.GetRequiredService<NexusPosDbContext>(), lowStockThreshold));
        services.AddScoped<DbInitializer>();
        return services;
    }
}
