using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using NexusPOS.Domain.Entities;
using NexusPOS.Domain.Enums;
using NexusPOS.Infrastructure.Persistence;

namespace NexusPOS.IntegrationTests;

public sealed class NexusPosWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"nexuspos-tests-{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
        });
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<NexusPosDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<NexusPosDbContext>>();
            services.RemoveAll<NexusPosDbContext>();
            services.AddDbContext<NexusPosDbContext>(options => options.UseInMemoryDatabase(_databaseName));
        });
    }

    public HttpClient CreateSeededClient()
    {
        var client = CreateClient();
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NexusPosDbContext>();
        dbContext.Database.EnsureCreated();
        if (dbContext.Users.Any())
        {
            return client;
        }

        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
        var admin = CreateUser(hasher, "Nexus", "Admin", "admin@test.local", "Admin123!", UserRole.Admin);
        var customer = CreateUser(hasher, "Cliente", "Uno", "customer@test.local", "Customer123!", UserRole.Customer);
        customer.Customer = new Customer { DocumentNumber = "10001", Phone = "3000000000", Address = "Bogotá" };
        var other = CreateUser(hasher, "Cliente", "Dos", "other@test.local", "Customer123!", UserRole.Customer);
        other.Customer = new Customer { DocumentNumber = "10002", Phone = "3000000001", Address = "Cali" };
        var category = new Category
        {
            Name = "Mouse",
            Description = "Mouse de prueba",
            ImageUrl = "https://example.test/mouse.jpg"
        };
        var product = new Product
        {
            Sku = "TEST-001",
            Name = "Mouse de prueba",
            Description = "Producto para pruebas de integración",
            Price = 100_000m,
            Stock = 5,
            Category = category
        };
        dbContext.AddRange(admin, customer, other, category, product);
        dbContext.SaveChanges();
        return client;
    }

    private static User CreateUser(IPasswordHasher<User> hasher, string firstName, string lastName, string email, string password, UserRole role)
    {
        var user = new User
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PasswordHash = string.Empty,
            Role = role
        };
        user.PasswordHash = hasher.HashPassword(user, password);
        return user;
    }
}
