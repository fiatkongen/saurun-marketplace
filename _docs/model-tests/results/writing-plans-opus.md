I'm using the writing-plans skill to route to the appropriate planner.

## Spec 1: "Add GET /api/users endpoint, UserEntity, UserService, authentication required"

**Route:** `saurun:dotnet-writing-plans`
**Context to load first:** `saurun:dotnet-tactical-ddd`
**Reasoning:** Every signal is backend: API endpoint (`GET /api/users`), Entity (`UserEntity`), Service (`UserService`), Authentication requirement. No frontend signals present. Pure backend work unit.

## Spec 2: "Create UserProfile component, form validation, state management with Zustand"

**Route:** `saurun:react-writing-plans`
**Context to load first:** `saurun:react-tailwind-v4-components`
**Reasoning:** Every signal is frontend: Component (`UserProfile component`), Forms (`form validation`), State management (`Zustand`). No backend signals present. Pure frontend work unit.

## Spec 3: "New project from scratch, needs API + React frontend + database schema"

**Route:** Both `saurun:dotnet-writing-plans` AND `saurun:react-writing-plans`, backend first.
**Context to load first:** `saurun:dotnet-tactical-ddd`, then `saurun:react-tailwind-v4-components`
**Reasoning:** Scaffold work unit — "New project from scratch" = no existing codebase. API + database schema = backend signals. React frontend = frontend signals.

**Order of operations:**
1. Load `saurun:dotnet-tactical-ddd` context
2. Run `saurun:dotnet-writing-plans` (API endpoints, database schema, domain model)
3. Load `saurun:react-tailwind-v4-components` context
4. Run `saurun:react-writing-plans` (depends on API contracts from step 2)

## Spec 4: "Add product filter UI with filters stored in query params, API returns filtered results"

**Route:** Both `saurun:dotnet-writing-plans` AND `saurun:react-writing-plans`, backend first.
**Context to load first:** `saurun:dotnet-tactical-ddd`, then `saurun:react-tailwind-v4-components`
**Reasoning:** Integration work unit. Backend signals: API returning filtered results. Frontend signals: Filter UI, query param state management.

**Order of operations:**
1. Load `saurun:dotnet-tactical-ddd` context
2. Run `saurun:dotnet-writing-plans` (filtering endpoint contract, query parameter schema)
3. Load `saurun:react-tailwind-v4-components` context
4. Run `saurun:react-writing-plans` (filter UI components, query param synchronization)

## Summary Table

| Spec | Work Unit Type | Planner(s) | Context Skill(s) | Order |
|------|---------------|-------------|-------------------|-------|
| 1 | Backend | `dotnet-writing-plans` | `dotnet-tactical-ddd` | Single |
| 2 | Frontend | `react-writing-plans` | `react-tailwind-v4-components` | Single |
| 3 | Scaffold | Both planners | Both context skills | Backend first |
| 4 | Integration | Both planners | Both context skills | Backend first |
