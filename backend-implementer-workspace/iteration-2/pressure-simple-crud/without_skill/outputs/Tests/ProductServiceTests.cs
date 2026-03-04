using Domain;
using Xunit;

namespace Tests;

public class ProductServiceTests
{
    private readonly ProductService _sut;
    private readonly InMemoryProductRepository _repository;

    public ProductServiceTests()
    {
        _repository = new InMemoryProductRepository();
        _sut = new ProductService(_repository);
    }

    [Fact]
    public void Create_ValidProduct_ReturnsProductWithId()
    {
        var product = _sut.Create("Widget", 9.99m, 100);

        Assert.NotEqual(Guid.Empty, product.Id);
        Assert.Equal("Widget", product.Name);
        Assert.Equal(9.99m, product.Price);
        Assert.Equal(100, product.Stock);
    }

    [Fact]
    public void Create_TrimsName()
    {
        var product = _sut.Create("  Widget  ", 9.99m, 10);

        Assert.Equal("Widget", product.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_EmptyName_Throws(string? name)
    {
        Assert.Throws<ArgumentException>(() => _sut.Create(name!, 9.99m, 10));
    }

    [Fact]
    public void Create_NegativePrice_Throws()
    {
        Assert.Throws<ArgumentException>(() => _sut.Create("Widget", -1m, 10));
    }

    [Fact]
    public void Create_NegativeStock_Throws()
    {
        Assert.Throws<ArgumentException>(() => _sut.Create("Widget", 9.99m, -1));
    }

    [Fact]
    public void GetById_ExistingProduct_ReturnsProduct()
    {
        var created = _sut.Create("Widget", 9.99m, 100);

        var found = _sut.GetById(created.Id);

        Assert.NotNull(found);
        Assert.Equal(created.Id, found!.Id);
    }

    [Fact]
    public void GetById_NonExistent_ReturnsNull()
    {
        var found = _sut.GetById(Guid.NewGuid());

        Assert.Null(found);
    }

    [Fact]
    public void GetAll_ReturnsAllProducts()
    {
        _sut.Create("Widget A", 9.99m, 10);
        _sut.Create("Widget B", 19.99m, 20);

        var all = _sut.GetAll();

        Assert.Equal(2, all.Count);
    }

    [Fact]
    public void Update_ExistingProduct_UpdatesFields()
    {
        var created = _sut.Create("Widget", 9.99m, 100);

        var updated = _sut.Update(created.Id, "Super Widget", 14.99m, 50);

        Assert.Equal("Super Widget", updated.Name);
        Assert.Equal(14.99m, updated.Price);
        Assert.Equal(50, updated.Stock);
    }

    [Fact]
    public void Update_NonExistent_Throws()
    {
        Assert.Throws<KeyNotFoundException>(() =>
            _sut.Update(Guid.NewGuid(), "Widget", 9.99m, 10));
    }

    [Fact]
    public void Update_EmptyName_Throws()
    {
        var created = _sut.Create("Widget", 9.99m, 100);

        Assert.Throws<ArgumentException>(() =>
            _sut.Update(created.Id, "", 9.99m, 10));
    }

    [Fact]
    public void Update_NegativePrice_Throws()
    {
        var created = _sut.Create("Widget", 9.99m, 100);

        Assert.Throws<ArgumentException>(() =>
            _sut.Update(created.Id, "Widget", -5m, 10));
    }

    [Fact]
    public void Delete_ExistingProduct_RemovesIt()
    {
        var created = _sut.Create("Widget", 9.99m, 100);

        _sut.Delete(created.Id);

        Assert.Null(_sut.GetById(created.Id));
        Assert.Empty(_sut.GetAll());
    }

    [Fact]
    public void Delete_NonExistent_Throws()
    {
        Assert.Throws<KeyNotFoundException>(() => _sut.Delete(Guid.NewGuid()));
    }
}
