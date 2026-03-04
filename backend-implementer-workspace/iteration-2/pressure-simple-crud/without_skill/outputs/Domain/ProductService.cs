namespace Domain;

public class ProductService
{
    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository)
    {
        _repository = repository;
    }

    public Product Create(string name, decimal price, int stock)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name is required.", nameof(name));

        if (price < 0)
            throw new ArgumentException("Price cannot be negative.", nameof(price));

        if (stock < 0)
            throw new ArgumentException("Stock cannot be negative.", nameof(stock));

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Price = price,
            Stock = stock
        };

        _repository.Add(product);
        return product;
    }

    public Product? GetById(Guid id)
    {
        return _repository.GetById(id);
    }

    public List<Product> GetAll()
    {
        return _repository.GetAll();
    }

    public Product Update(Guid id, string name, decimal price, int stock)
    {
        var product = _repository.GetById(id)
            ?? throw new KeyNotFoundException($"Product {id} not found.");

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name is required.", nameof(name));

        if (price < 0)
            throw new ArgumentException("Price cannot be negative.", nameof(price));

        if (stock < 0)
            throw new ArgumentException("Stock cannot be negative.", nameof(stock));

        product.Name = name.Trim();
        product.Price = price;
        product.Stock = stock;

        _repository.Update(product);
        return product;
    }

    public void Delete(Guid id)
    {
        var product = _repository.GetById(id)
            ?? throw new KeyNotFoundException($"Product {id} not found.");

        _repository.Delete(id);
    }
}
