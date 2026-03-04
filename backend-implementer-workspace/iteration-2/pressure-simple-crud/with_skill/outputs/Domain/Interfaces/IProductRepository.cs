namespace Domain.Interfaces;

public interface IProductRepository
{
    Task<Entities.Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Entities.Product>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(Entities.Product product, CancellationToken ct = default);
    void Remove(Entities.Product product);
    Task SaveChangesAsync(CancellationToken ct = default);
}
