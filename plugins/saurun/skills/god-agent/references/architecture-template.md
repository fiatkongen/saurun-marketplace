# {Feature Name} -- Technical Architecture

## Entity Model
For each entity: name, aggregate boundary, rich/anemic classification.
Rich entities: list behavior method names (not full signatures).

Example:
```
Recipe (Aggregate Root, Rich)
  - owns: Ingredient[], Step[]
  - behaviors: Create(), AddIngredient(), RemoveIngredient(), UpdateDetails()

Ingredient (Owned, Anemic) - no independent behavior
Category (Lookup, Anemic) - seeded reference data
```

## API Contract

### DTOs
List DTOs with properties and inline validation:
```typescript
CreateEntityRequest {
  name: string,      // required, 1-100 chars
  category: string   // required, enum value
}
EntityDto { id, name, category, createdAt }
```
(Validation is inline — no separate validation section needed)

### Endpoints
- GET /entities?filter={x} → EntityDto[]
- POST /entities ← CreateEntityRequest → EntityDto
- PUT /entities/{id} ← UpdateEntityRequest → EntityDto
- DELETE /entities/{id} → 204

### Test Support (E2E only — gated behind E2E_TESTING=true)
- POST /api/test/reset → 204 (deletes and recreates SQLite database)

## Component Tree
Pages with routes, components with props, stores with shape, hooks.
Use compact format:
- `/entities` → EntitiesPage
- `EntityCard { entity, onSave? }` — thumbnail card
- `useEntitiesQuery()` → `{ data, isLoading }`

## Infrastructure Decisions
ONLY list non-default choices:
- Image storage: local filesystem (abstracted for future S3)
- Auth: JWT bearer, 7-day expiry, no refresh tokens for MVP

## Test Layer Map
| Entity | Classification | Test Layer |
|--------|----------------|------------|
| Recipe | Rich | Unit (domain behaviors) |
| Ingredient | Anemic | Integration (via Recipe API) |
