using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NexusPOS.Domain.Entities;
using NexusPOS.Domain.Enums;

namespace NexusPOS.Infrastructure.Persistence;
// Prepara la base al arrancar: aplica migraciones, completa imágenes antiguas y
// crea datos demostrativos únicamente cuando todavía no existen usuarios.
public sealed class DbInitializer(
    NexusPosDbContext dbContext,
    IPasswordHasher<User> passwordHasher,
    IConfiguration configuration,
    IHostEnvironment environment,
    ILogger<DbInitializer> logger)
{
    private static readonly IReadOnlyDictionary<string, string> CategoryImages = new Dictionary<string, string>
    {
        ["Mouse"] = "https://images.unsplash.com/photo-1527814050087-3793815479db?auto=format&fit=crop&w=1200&q=85",
        ["Teclados"] = "https://images.unsplash.com/photo-1587829741301-dc798b83add3?auto=format&fit=crop&w=1200&q=85",
        ["Audio"] = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?auto=format&fit=crop&w=1200&q=85",
        ["Monitores"] = "https://images.unsplash.com/photo-1527443224154-c4a3942d3acf?auto=format&fit=crop&w=1200&q=85",
        ["Accesorios"] = "https://images.unsplash.com/photo-1592840496694-26d035b52b48?auto=format&fit=crop&w=1200&q=85"
    };

    // Sincroniza el esquema y garantiza un conjunto inicial reproducible.
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (dbContext.Database.IsRelational())
        {
            // En MySQL se aplican solo las migraciones pendientes registradas por EF.
            await dbContext.Database.MigrateAsync(cancellationToken);
        }
        else
        {
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        }

        await EnsureCategoryImagesAsync(cancellationToken);

        // El seed principal es idempotente: no duplica información al reiniciar la API.
        if (await dbContext.Users.AnyAsync(cancellationToken))
        {
            return;
        }

        logger.LogInformation("Inicializando datos de desarrollo de NexusPOS");
        var adminEmail = configuration["Seed:AdminEmail"] ?? "admin@nexuspos.local";
        var adminPassword = configuration["Seed:AdminPassword"];
        var customerPassword = configuration["Seed:CustomerPassword"];
        if (string.IsNullOrWhiteSpace(adminPassword) || string.IsNullOrWhiteSpace(customerPassword))
        {
            if (!environment.IsDevelopment() && !environment.IsEnvironment("Testing"))
            {
                // Fuera de entornos locales nunca se permiten contraseñas predeterminadas.
                throw new InvalidOperationException("Las contraseñas de seed deben configurarse mediante variables de entorno.");
            }

            adminPassword ??= "Admin123!";
            customerPassword ??= "Customer123!";
        }

        var admin = CreateUser("Nexus", "Admin", adminEmail, adminPassword, UserRole.Admin);
        var customers = new[]
        {
            CreateCustomer("Laura", "Gómez", "laura@nexuspos.local", customerPassword, "1001001001", "3001112233", "Bogotá"),
            CreateCustomer("Carlos", "Rojas", "carlos@nexuspos.local", customerPassword, "1001001002", "3001112244", "Medellín"),
            CreateCustomer("Ana", "Martínez", "ana@nexuspos.local", customerPassword, "1001001003", "3001112255", "Cali")
        };

        var categories = new[]
        {
            new Category { Name = "Mouse", Description = "Mouse alámbricos e inalámbricos", ImageUrl = CategoryImages["Mouse"] },
            new Category { Name = "Teclados", Description = "Teclados mecánicos y de oficina", ImageUrl = CategoryImages["Teclados"] },
            new Category { Name = "Audio", Description = "Audífonos, micrófonos y parlantes", ImageUrl = CategoryImages["Audio"] },
            new Category { Name = "Monitores", Description = "Monitores para productividad y gaming", ImageUrl = CategoryImages["Monitores"] },
            new Category { Name = "Accesorios", Description = "Webcams, controles y accesorios", ImageUrl = CategoryImages["Accesorios"] }
        };
        var products = CreateProducts(categories);

        dbContext.Users.Add(admin);
        dbContext.Users.AddRange(customers);
        dbContext.Categories.AddRange(categories);
        dbContext.Products.AddRange(products);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    // Completa ImageUrl en categorías creadas antes de la migración de imágenes.
    private async Task EnsureCategoryImagesAsync(CancellationToken cancellationToken)
    {
        var categoriesWithoutImage = await dbContext.Categories
            .Where(x => x.ImageUrl == null || x.ImageUrl == string.Empty)
            .ToListAsync(cancellationToken);
        var changed = false;
        foreach (var category in categoriesWithoutImage)
        {
            if (!CategoryImages.TryGetValue(category.Name, out var imageUrl))
            {
                continue;
            }

            category.ImageUrl = imageUrl;
            changed = true;
        }

        if (changed)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    // Crea un usuario y calcula inmediatamente su hash de contraseña.
    private User CreateUser(string firstName, string lastName, string email, string password, UserRole role)
    {
        var user = new User
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = string.Empty,
            Role = role
        };
        user.PasswordHash = passwordHasher.HashPassword(user, password);
        return user;
    }

    // Crea un User Customer junto con su perfil comercial uno-a-uno.
    private User CreateCustomer(string firstName, string lastName, string email, string password, string document, string phone, string address)
    {
        var user = CreateUser(firstName, lastName, email, password, UserRole.Customer);
        user.Customer = new Customer { DocumentNumber = document, Phone = phone, Address = address };
        return user;
    }

    // Construye el catálogo demostrativo enlazando cada producto con su categoría.
    private static Product[] CreateProducts(Category[] categories)
    {
        var data = new (string Sku, string Name, string Description, decimal Price, int Stock, int Category, string Image)[]
        {
            ("MOU-G502", "Logitech G502 Hero", "Mouse gaming de alto rendimiento con sensor HERO.", 249900, 12, 0, "https://images.unsplash.com/photo-1527814050087-3793815479db?auto=format&fit=crop&w=800&q=80"),
            ("MOU-MX3S", "Logitech MX Master 3S", "Mouse inalámbrico silencioso para productividad.", 499900, 8, 0, "https://images.unsplash.com/photo-1615663245857-ac93bb7c39e7?auto=format&fit=crop&w=800&q=80"),
            ("MOU-RV2", "Razer Viper V2 Pro", "Mouse ultraligero competitivo.", 599900, 5, 0, "https://images.unsplash.com/photo-1563297007-0686b7003af7?auto=format&fit=crop&w=800&q=80"),
            ("KEY-K2", "Keychron K2", "Teclado mecánico inalámbrico compacto.", 449900, 9, 1, "https://images.unsplash.com/photo-1587829741301-dc798b83add3?auto=format&fit=crop&w=800&q=80"),
            ("KEY-BW4", "Razer BlackWidow V4", "Teclado mecánico gaming RGB.", 749900, 4, 1, "https://images.unsplash.com/photo-1595225476474-87563907a212?auto=format&fit=crop&w=800&q=80"),
            ("KEY-MX", "Logitech MX Keys S", "Teclado inalámbrico de perfil bajo.", 599900, 7, 1, "https://images.unsplash.com/photo-1618384887929-16ec33fab9ef?auto=format&fit=crop&w=800&q=80"),
            ("AUD-HXC3", "HyperX Cloud III", "Audífonos gaming con audio espacial.", 429900, 10, 2, "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?auto=format&fit=crop&w=800&q=80"),
            ("MIC-HXQ", "HyperX QuadCast", "Micrófono USB para streaming.", 699900, 3, 2, "https://images.unsplash.com/photo-1590602847861-f357a9332bbc?auto=format&fit=crop&w=800&q=80"),
            ("AUD-SNY", "Sony WH-1000XM5", "Audífonos con cancelación de ruido.", 1599900, 6, 2, "https://images.unsplash.com/photo-1484704849700-f032a568e944?auto=format&fit=crop&w=800&q=80"),
            ("MON-OG5", "Samsung Odyssey G5", "Monitor gaming QHD de 27 pulgadas.", 1499900, 5, 3, "https://images.unsplash.com/photo-1527443224154-c4a3942d3acf?auto=format&fit=crop&w=800&q=80"),
            ("MON-LG29", "LG UltraWide 29", "Monitor ultrawide IPS para productividad.", 1299900, 4, 3, "https://images.unsplash.com/photo-1547119957-637f8679db1e?auto=format&fit=crop&w=800&q=80"),
            ("MON-AS24", "ASUS TUF 24", "Monitor Full HD de 165 Hz.", 999900, 7, 3, "https://images.unsplash.com/photo-1585792180666-f7347c490ee2?auto=format&fit=crop&w=800&q=80"),
            ("WEB-C920", "Logitech C920", "Webcam Full HD para videollamadas.", 329900, 11, 4, "https://images.unsplash.com/photo-1587826080692-f439cd0b70da?auto=format&fit=crop&w=800&q=80"),
            ("CTL-XBX", "Xbox Wireless Controller", "Control inalámbrico para Xbox y PC.", 299900, 8, 4, "https://images.unsplash.com/photo-1592840496694-26d035b52b48?auto=format&fit=crop&w=800&q=80"),
            ("HUB-USBC", "Hub USB-C 8 en 1", "Hub multipuerto con HDMI y lector SD.", 189900, 15, 4, "https://images.unsplash.com/photo-1625842268584-8f3296236761?auto=format&fit=crop&w=800&q=80")
        };

        return data.Select(item => new Product
        {
            Sku = item.Sku,
            Name = item.Name,
            Description = item.Description,
            Price = item.Price,
            Stock = item.Stock,
            Category = categories[item.Category],
            ImageUrl = item.Image
        }).ToArray();
    }
}
