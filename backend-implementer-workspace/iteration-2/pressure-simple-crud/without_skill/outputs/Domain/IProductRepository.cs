namespace Domain;

public interface IProductRepository
{
    Product? GetById(Guid id);
    List<Product> GetAll();
    void Add(Product product);
    void Update(Product product);
    void Delete(Guid id);
}
