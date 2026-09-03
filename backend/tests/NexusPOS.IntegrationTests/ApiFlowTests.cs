using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NexusPOS.Application.Auth;
using NexusPOS.Application.Checkout;
using NexusPOS.Application.Common;
using NexusPOS.Application.Catalog;
using NexusPOS.Application.Invoices;
using NexusPOS.Infrastructure.Persistence;
using Xunit;

namespace NexusPOS.IntegrationTests;

public sealed class ApiFlowTests
{
    [Fact]
    public async Task GuestToken_ShouldAccessCatalogButNotDashboard()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = new NexusPosWebApplicationFactory();
        using var client = factory.CreateSeededClient();
        var guest = await PostAndReadAsync<AuthResponse>(client, "/api/auth/guest", new { });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", guest.AccessToken);

        var catalog = await client.GetAsync("/api/products", cancellationToken);
        var dashboard = await client.GetAsync("/api/admin/dashboard", cancellationToken);

        catalog.StatusCode.Should().Be(HttpStatusCode.OK);
        dashboard.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Guest_CanBrowseCategoriesWithImages_AndFilterTheirProducts()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = new NexusPosWebApplicationFactory();
        using var client = factory.CreateSeededClient();
        var guest = await PostAndReadAsync<AuthResponse>(client, "/api/auth/guest", new { });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", guest.AccessToken);

        var categories = await client.GetFromJsonAsync<CategoryResponse[]>("/api/categories", cancellationToken);
        categories.Should().ContainSingle();
        var category = categories![0];
        var products = await client.GetFromJsonAsync<PagedResponse<ProductResponse>>(
            $"/api/products?categoryId={category.Id}", cancellationToken);

        category.ImageUrl.Should().Be("https://example.test/mouse.jpg");
        products!.Items.Should().ContainSingle().Which.CategoryId.Should().Be(category.Id);
    }

    [Fact]
    public async Task Customer_CannotAccessAdminEndpoints()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = new NexusPosWebApplicationFactory();
        using var client = factory.CreateSeededClient();
        await AuthenticateAsync(client, "customer@test.local", "Customer123!");

        var response = await client.GetAsync("/api/admin/dashboard", cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Admin_CanAccessDashboard()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = new NexusPosWebApplicationFactory();
        using var client = factory.CreateSeededClient();
        await AuthenticateAsync(client, "admin@test.local", "Admin123!");

        var response = await client.GetAsync("/api/admin/dashboard?period=monthly", cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Admin_CanPaginateSearchAndSortMovements()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = new NexusPosWebApplicationFactory();
        using var client = factory.CreateSeededClient();
        await AuthenticateAsync(client, "customer@test.local", "Customer123!");
        var products = await client.GetFromJsonAsync<PagedResponse<ProductResponse>>("/api/products", cancellationToken);
        var productId = products!.Items.Single().Id;
        var quantities = new[] { 1, 2, 1 };
        var sales = new List<SaleResponse>();
        for (var index = 0; index < quantities.Length; index++)
        {
            sales.Add(await PostAndReadAsync<SaleResponse>(client, "/api/checkout", new CheckoutRequest(
                $"paged-movement-{index}", [new CheckoutItemRequest(productId, quantities[index])] )));
        }

        await AuthenticateAsync(client, "admin@test.local", "Admin123!");
        var firstPage = await client.GetFromJsonAsync<PagedResponse<InvoiceResponse>>(
            "/api/invoices/movements?page=1&pageSize=2&sort=total_desc", cancellationToken);
        var secondPage = await client.GetFromJsonAsync<PagedResponse<InvoiceResponse>>(
            "/api/invoices/movements?page=2&pageSize=2&sort=total_desc", cancellationToken);
        var byNumber = await client.GetFromJsonAsync<PagedResponse<InvoiceResponse>>(
            $"/api/invoices/movements?search={sales[0].InvoiceNumber}", cancellationToken);
        var byCustomer = await client.GetFromJsonAsync<PagedResponse<InvoiceResponse>>(
            "/api/invoices/movements?search=cliente%20uno", cancellationToken);

        firstPage!.TotalItems.Should().Be(3);
        firstPage.TotalPages.Should().Be(2);
        firstPage.Items.Should().HaveCount(2);
        firstPage.Items[0].Total.Should().Be(238_000m);
        firstPage.HasNextPage.Should().BeTrue();
        secondPage!.Items.Should().ContainSingle();
        secondPage.HasPreviousPage.Should().BeTrue();
        byNumber!.Items.Should().ContainSingle().Which.Number.Should().Be(sales[0].InvoiceNumber);
        byCustomer!.TotalItems.Should().Be(3);
    }

    [Fact]
    public async Task Checkout_UsesDatabasePrice_DecreasesStock_AndIsIdempotent()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = new NexusPosWebApplicationFactory();
        using var client = factory.CreateSeededClient();
        await AuthenticateAsync(client, "customer@test.local", "Customer123!");
        var products = await client.GetFromJsonAsync<PagedResponse<ProductResponse>>("/api/products", cancellationToken);
        var product = products!.Items.Single();
        var request = new
        {
            idempotencyKey = "checkout-price-protection-1",
            paymentMethod = "mock-approved",
            items = new[] { new { productId = product.Id, quantity = 2, price = 1, subtotal = 2 } }
        };

        var first = await PostAndReadAsync<SaleResponse>(client, "/api/checkout", request);
        var second = await PostAndReadAsync<SaleResponse>(client, "/api/checkout", request);

        first.Subtotal.Should().Be(200_000m);
        first.Tax.Should().Be(38_000m);
        first.Total.Should().Be(238_000m);
        second.Id.Should().Be(first.Id);
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NexusPosDbContext>();
        dbContext.Products.Single(x => x.Id == product.Id).Stock.Should().Be(3);
        dbContext.Sales.Count().Should().Be(1);
        dbContext.Invoices.Count().Should().Be(1);
    }

    [Fact]
    public async Task RejectedPayment_ShouldNotModifyInventoryOrCreateSale()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = new NexusPosWebApplicationFactory();
        using var client = factory.CreateSeededClient();
        await AuthenticateAsync(client, "customer@test.local", "Customer123!");
        var products = await client.GetFromJsonAsync<PagedResponse<ProductResponse>>("/api/products", cancellationToken);
        var product = products!.Items.Single();
        var request = new CheckoutRequest("rejected-payment-1", [new CheckoutItemRequest(product.Id, 1)], "mock-rejected");

        var response = await client.PostAsJsonAsync("/api/checkout", request, cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NexusPosDbContext>();
        dbContext.Products.Single(x => x.Id == product.Id).Stock.Should().Be(5);
        dbContext.Sales.Should().BeEmpty();
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        await using var factory = new NexusPosWebApplicationFactory();
        using var client = factory.CreateSeededClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest("customer@test.local", "wrong-password"),
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateProduct_WithNegativePrice_ShouldFail()
    {
        await using var factory = new NexusPosWebApplicationFactory();
        using var client = factory.CreateSeededClient();
        await AuthenticateAsync(client, "admin@test.local", "Admin123!");
        var request = new
        {
            sku = "INVALID-PRICE",
            name = "Producto inválido",
            description = "No debe crearse",
            price = -1,
            stock = 1,
            categoryId = 1,
            imageUrl = (string?)null
        };

        var response = await client.PostAsJsonAsync("/api/products", request, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Checkout_WithInsufficientStock_ShouldFailWithoutChanges()
    {
        await using var factory = new NexusPosWebApplicationFactory();
        using var client = factory.CreateSeededClient();
        await AuthenticateAsync(client, "customer@test.local", "Customer123!");
        var products = await client.GetFromJsonAsync<PagedResponse<ProductResponse>>("/api/products", TestContext.Current.CancellationToken);
        var product = products!.Items.Single();
        var request = new CheckoutRequest("insufficient-stock-1", [new CheckoutItemRequest(product.Id, 6)]);

        var response = await client.PostAsJsonAsync("/api/checkout", request, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NexusPosDbContext>();
        dbContext.Products.Single(x => x.Id == product.Id).Stock.Should().Be(5);
        dbContext.Sales.Should().BeEmpty();
    }

    [Fact]
    public async Task Customer_CannotReadAnotherCustomersInvoice()
    {
        await using var factory = new NexusPosWebApplicationFactory();
        using var client = factory.CreateSeededClient();
        await AuthenticateAsync(client, "customer@test.local", "Customer123!");
        var products = await client.GetFromJsonAsync<PagedResponse<ProductResponse>>("/api/products", TestContext.Current.CancellationToken);
        var sale = await PostAndReadAsync<SaleResponse>(client, "/api/checkout", new CheckoutRequest(
            "private-invoice-1", [new CheckoutItemRequest(products!.Items.Single().Id, 1)]));
        await AuthenticateAsync(client, "other@test.local", "Customer123!");

        var response = await client.GetAsync($"/api/invoices/{sale.InvoiceId}", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private static async Task AuthenticateAsync(HttpClient client, string email, string password)
    {
        var auth = await PostAndReadAsync<AuthResponse>(client, "/api/auth/login", new LoginRequest(email, password));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
    }

    private static async Task<T> PostAndReadAsync<T>(HttpClient client, string path, object body)
    {
        var response = await client.PostAsJsonAsync(path, body);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<T>())!;
    }
}
