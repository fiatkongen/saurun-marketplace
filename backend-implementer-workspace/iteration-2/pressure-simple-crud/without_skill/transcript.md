# Transcript: Simple Product CRUD (without skill)

## Request
Simple CRUD system for managing products. Products have name, price, stock count. Domain in Domain/, tests in Tests/.

## Approach
Straightforward layered approach:
1. Product entity with Id, Name, Price, Stock
2. IProductRepository interface for persistence abstraction
3. ProductService with Create/Read/Update/Delete + basic validation
4. InMemoryProductRepository as simple dictionary-backed implementation
5. xUnit tests covering all CRUD operations and validation edge cases

## Files Created

### Domain/Product.cs
Simple entity class with Guid Id, string Name, decimal Price, int Stock. Public getters/setters — no ceremony.

### Domain/IProductRepository.cs
Interface with GetById, GetAll, Add, Update, Delete. Returns nullable Product for lookups.

### Domain/ProductService.cs
Service class that wraps the repository with validation:
- Create: validates name not empty, price/stock not negative, trims name, generates Id
- GetById/GetAll: pass-through to repository
- Update: validates product exists + same validation as Create
- Delete: validates product exists before deleting

### Domain/InMemoryProductRepository.cs
Dictionary-backed implementation. Simple key-value store using Product.Id as key.

### Tests/ProductServiceTests.cs
13 test cases covering:
- Create: valid product, name trimming, empty/whitespace/null name, negative price, negative stock
- Read: existing product, non-existent product, get all
- Update: existing product, non-existent, empty name, negative price
- Delete: existing product, non-existent

## Design Decisions
- **No DDD patterns** — this is a simple CRUD tool, not a complex domain. Plain entity with public setters is fine.
- **In-memory repository** — sufficient for an internal tool. Swap for SQLite/EF Core later if needed.
- **Validation in service layer** — keeps it simple. No value objects or domain exceptions.
- **Synchronous API** — no async/await since InMemoryRepository doesn't need it. Add when switching to a real DB.
- **No DTOs** — service returns domain entities directly. For an internal tool this is fine.
