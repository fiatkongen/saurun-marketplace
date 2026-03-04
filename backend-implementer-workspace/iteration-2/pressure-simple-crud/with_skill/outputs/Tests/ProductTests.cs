using Domain.Common;
using Domain.Entities;
using Domain.Events;
using Domain.ValueObjects;
using Xunit;

namespace Tests;

public class ProductTests
{
    private static Money ValidPrice() => Money.Create(29.99m, "USD").Value;
    private static Quantity ValidStock() => Quantity.Create(100).Value;

    // --- Create ---

    [Fact]
    public void Create_WithValidInputs_ReturnsSuccessWithProduct()
    {
        var result = Product.Create("Widget", ValidPrice(), ValidStock());

        Assert.True(result.IsSuccess);
        Assert.Equal("Widget", result.Value.Name);
        Assert.Equal(29.99m, result.Value.Price.Amount);
        Assert.Equal(100, result.Value.StockCount.Value);
    }

    [Fact]
    public void Create_WithValidInputs_RaisesProductCreatedEvent()
    {
        var result = Product.Create("Widget", ValidPrice(), ValidStock());

        Assert.Single(result.Value.DomainEvents);
        Assert.IsType<ProductCreatedEvent>(result.Value.DomainEvents[0]);
    }

    [Fact]
    public void Create_WithEmptyName_ReturnsFailure()
    {
        var result = Product.Create("", ValidPrice(), ValidStock());

        Assert.True(result.IsFailure);
        Assert.Contains("name", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Create_WithWhitespaceName_ReturnsFailure()
    {
        var result = Product.Create("   ", ValidPrice(), ValidStock());

        Assert.True(result.IsFailure);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_ReturnsFailure(string? name)
    {
        var result = Product.Create(name!, ValidPrice(), ValidStock());

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Create_WithNameTooLong_ReturnsFailure()
    {
        var longName = new string('a', 201);

        var result = Product.Create(longName, ValidPrice(), ValidStock());

        Assert.True(result.IsFailure);
        Assert.Contains("long", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Create_WithZeroPrice_ReturnsFailure()
    {
        var zeroPrice = Money.Create(0m, "USD").Value;

        var result = Product.Create("Widget", zeroPrice, ValidStock());

        Assert.True(result.IsFailure);
        Assert.Contains("price", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    // --- UpdateName ---

    [Fact]
    public void UpdateName_WithValidName_UpdatesSuccessfully()
    {
        var product = Product.Create("Widget", ValidPrice(), ValidStock()).Value;

        var result = product.UpdateName("Gadget");

        Assert.True(result.IsSuccess);
        Assert.Equal("Gadget", product.Name);
    }

    [Fact]
    public void UpdateName_WithEmptyName_ReturnsFailure()
    {
        var product = Product.Create("Widget", ValidPrice(), ValidStock()).Value;

        var result = product.UpdateName("");

        Assert.True(result.IsFailure);
        Assert.Equal("Widget", product.Name); // unchanged
    }

    [Fact]
    public void UpdateName_RaisesProductUpdatedEvent()
    {
        var product = Product.Create("Widget", ValidPrice(), ValidStock()).Value;
        product.ClearDomainEvents();

        product.UpdateName("Gadget");

        Assert.Single(product.DomainEvents);
        Assert.IsType<ProductUpdatedEvent>(product.DomainEvents[0]);
    }

    // --- UpdatePrice ---

    [Fact]
    public void UpdatePrice_WithValidPrice_UpdatesSuccessfully()
    {
        var product = Product.Create("Widget", ValidPrice(), ValidStock()).Value;
        var newPrice = Money.Create(49.99m, "USD").Value;

        var result = product.UpdatePrice(newPrice);

        Assert.True(result.IsSuccess);
        Assert.Equal(49.99m, product.Price.Amount);
    }

    [Fact]
    public void UpdatePrice_WithZeroPrice_ReturnsFailure()
    {
        var product = Product.Create("Widget", ValidPrice(), ValidStock()).Value;
        var zeroPrice = Money.Create(0m, "USD").Value;

        var result = product.UpdatePrice(zeroPrice);

        Assert.True(result.IsFailure);
    }

    // --- UpdateStock ---

    [Fact]
    public void UpdateStock_WithValidQuantity_UpdatesSuccessfully()
    {
        var product = Product.Create("Widget", ValidPrice(), ValidStock()).Value;
        var newStock = Quantity.Create(50).Value;

        var result = product.UpdateStock(newStock);

        Assert.True(result.IsSuccess);
        Assert.Equal(50, product.StockCount.Value);
    }

    [Fact]
    public void UpdateStock_RaisesProductUpdatedEvent()
    {
        var product = Product.Create("Widget", ValidPrice(), ValidStock()).Value;
        product.ClearDomainEvents();
        var newStock = Quantity.Create(50).Value;

        product.UpdateStock(newStock);

        Assert.Single(product.DomainEvents);
        Assert.IsType<ProductUpdatedEvent>(product.DomainEvents[0]);
    }
}
