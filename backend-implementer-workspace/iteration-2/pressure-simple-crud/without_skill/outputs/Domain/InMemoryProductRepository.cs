namespace Domain;

public class InMemoryProductRepository : IProductRepository
{
    private readonly Dictionary<Guid, Product> _products = new();

    public Product? GetById(Guid id)
    {
        _products.TryGetValue(id, out var product);
        return product;
    }

    public List<Product> GetAll()
    {
        return _products.Values.ToList();
    }

    public void Add(Product product)
    {
        _products[product.Id] = product;
    }

    public void Update(Product product)
    {
        if (!_products.ContainsKey(product.Id))
            throw new KeyNotFoundException($"Product {product.Id} not found.");

        _products[product.Id] = product;
    }

    public void Delete(Guid id)
    {
        _products.Remove(id);
    }
}
