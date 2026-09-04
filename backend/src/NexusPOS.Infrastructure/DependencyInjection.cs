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
// Composition Root de Infrastructure: conecta las interfaces de Application con
// implementaciones concretas y configura MySQL, JWT y reglas parametrizables.
public static class DependencyInjection
{
    // Registra toda la infraestructura necesaria para ejecutar NexusPOS.
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // DbContext es Scoped: cada petición HTTP recibe una unidad de trabajo independiente.
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection no está configurada.");
        services.AddDbContext<NexusPosDbContext>(options => options.UseMySQL(connectionString));

        // Se valida el secreto durante el arranque para fallar rápido ante una configuración insegura.
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException("La configuración JWT no está disponible.");
        if (jwt.Secret.Length < 32)
        {
            throw new InvalidOperationException("JWT_SECRET debe contener al menos 32 caracteres.");
        }

        // Cada JWT debe tener emisor, audiencia, vigencia y firma válidos.
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

        // Las reglas configurables se leen una vez y se inyectan en los servicios que las aplican.
        var taxRate = configuration.GetValue("Business:TaxRate", 0.19m);
        var lowStockThreshold = configuration.GetValue("Business:LowStockThreshold", 5);
        // Mapa interfaz → implementación utilizado por los controllers y servicios.
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
