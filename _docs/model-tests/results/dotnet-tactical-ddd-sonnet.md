Complete tactical DDD implementation with:
- Money value object (private ctor, Result<T> factory, GetEqualityComponents)
- Product entity (private setters, Create() factory, UpdatePrice() behavior method returning Result)
- IProductRepository (specific interface)
- ProductResponse, CreateProductRequest, UpdateProductPriceRequest (immutable records)
- ProductMappingExtensions (ToDto(), ToDomain(), UpdateFromRequest() extension methods, no AutoMapper)
- ProductConfiguration (EF Core OwnsOne for Money, column mapping with precision)
- ProductsController (returns DTOs only, uses Result for validation)
- Protected parameterless constructor for EF Core
- Dependency rule respected
