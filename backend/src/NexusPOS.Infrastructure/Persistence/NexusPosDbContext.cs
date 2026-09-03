using Microsoft.EntityFrameworkCore;
using NexusPOS.Domain.Entities;

namespace NexusPOS.Infrastructure.Persistence;

public sealed class NexusPosDbContext(DbContextOptions<NexusPosDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Invoice> Invoices => Set<Invoice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.Email).HasMaxLength(254);
            entity.Property(x => x.FirstName).HasMaxLength(80);
            entity.Property(x => x.LastName).HasMaxLength(80);
            entity.Property(x => x.PasswordHash).HasMaxLength(500);
            entity.Property(x => x.Role).HasConversion<string>().HasMaxLength(20);
            entity.HasOne(x => x.Customer).WithOne(x => x.User).HasForeignKey<Customer>(x => x.UserId);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasIndex(x => x.DocumentNumber).IsUnique();
            entity.Property(x => x.DocumentNumber).HasMaxLength(30);
            entity.Property(x => x.Phone).HasMaxLength(30);
            entity.Property(x => x.Address).HasMaxLength(250);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasIndex(x => x.Name).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(100);
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.ImageUrl).HasMaxLength(1000);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(x => x.Sku).IsUnique();
            entity.HasIndex(x => x.CreatedAt);
            entity.HasIndex(x => new { x.CategoryId, x.IsActive });
            entity.Property(x => x.Sku).HasMaxLength(50);
            entity.Property(x => x.Name).HasMaxLength(160);
            entity.Property(x => x.Description).HasMaxLength(2000);
            entity.Property(x => x.ImageUrl).HasMaxLength(1000);
            entity.Property(x => x.Price).HasPrecision(18, 2);
            entity.ToTable(table =>
            {
                table.HasCheckConstraint("CK_Products_Price_Positive", "`Price` > 0");
                table.HasCheckConstraint("CK_Products_Stock_NonNegative", "`Stock` >= 0");
            });
        });

        modelBuilder.Entity<Sale>(entity =>
        {
            entity.HasIndex(x => x.InvoiceNumber).IsUnique();
            entity.HasIndex(x => x.CreatedAt);
            entity.HasIndex(x => new { x.CustomerId, x.IdempotencyKey }).IsUnique();
            entity.Property(x => x.InvoiceNumber).HasMaxLength(30);
            entity.Property(x => x.IdempotencyKey).HasMaxLength(100);
            entity.Property(x => x.Subtotal).HasPrecision(18, 2);
            entity.Property(x => x.Tax).HasPrecision(18, 2);
            entity.Property(x => x.Discount).HasPrecision(18, 2);
            entity.Property(x => x.Total).HasPrecision(18, 2);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        });

        modelBuilder.Entity<SaleItem>(entity =>
        {
            entity.Property(x => x.ProductNameSnapshot).HasMaxLength(160);
            entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
            entity.Property(x => x.Subtotal).HasPrecision(18, 2);
            entity.HasOne(x => x.Product).WithMany(x => x.SaleItems).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasIndex(x => x.ProviderReference).IsUnique();
            entity.Property(x => x.Provider).HasMaxLength(50);
            entity.Property(x => x.ProviderReference).HasMaxLength(100);
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.Currency).HasMaxLength(3);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
            entity.HasOne(x => x.Sale).WithOne(x => x.Payment).HasForeignKey<Payment>(x => x.SaleId);
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasIndex(x => x.Number).IsUnique();
            entity.Property(x => x.Number).HasMaxLength(30);
            entity.Property(x => x.CustomerNameSnapshot).HasMaxLength(180);
            entity.Property(x => x.CustomerDocumentSnapshot).HasMaxLength(30);
            entity.HasOne(x => x.Sale).WithOne(x => x.Invoice).HasForeignKey<Invoice>(x => x.SaleId);
        });
    }
}
