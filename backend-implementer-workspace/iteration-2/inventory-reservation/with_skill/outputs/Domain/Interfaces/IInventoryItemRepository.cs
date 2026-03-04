using InventoryReservation.Domain.Entities;
using InventoryReservation.Domain.ValueObjects;

namespace InventoryReservation.Domain.Interfaces;

public interface IInventoryItemRepository
{
    Task<InventoryItem?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<InventoryItem?> GetByProductIdAsync(ProductId productId, CancellationToken ct = default);
    Task AddAsync(InventoryItem item, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
