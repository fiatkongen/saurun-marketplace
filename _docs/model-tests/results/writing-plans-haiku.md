I'm using the writing-plans skill to route to the appropriate planner.

## Spec 1: "Add GET /api/users endpoint, UserEntity, UserService, authentication required"

**Routing Decision:** `saurun:dotnet-writing-plans`
**Context Skill(s) to Load First:** `saurun:dotnet-tactical-ddd`
**Reasoning:** Signals: API endpoint, UserEntity (domain model), UserService, authentication logic. All backend-specific patterns. No frontend/UI components mentioned.

## Spec 2: "Create UserProfile component, form validation, state management with Zustand"

**Routing Decision:** `saurun:react-writing-plans`
**Context Skill(s) to Load First:** `saurun:react-tailwind-v4-components`
**Reasoning:** Signals: Component creation, form validation, state management (Zustand). Pure frontend work; no backend domain logic or API contracts.

## Spec 3: "New project from scratch, needs API + React frontend + database schema"

**Routing Decision:** Both planners (backend first, then frontend)
**Context Skill(s) to Load First:** Both `saurun:dotnet-tactical-ddd` and `saurun:react-tailwind-v4-components`

**Order of Operations:**
1. Load `saurun:dotnet-tactical-ddd`
2. Invoke `saurun:dotnet-writing-plans` (database schema, API design, domain model)
3. Load `saurun:react-tailwind-v4-components`
4. Invoke `saurun:react-writing-plans` (frontend plan depends on API contracts from step 2)

**Reasoning:** Scaffold signal: new project from scratch. Full-stack work; must plan backend/API first.

## Spec 4: "Add product filter UI with filters stored in query params, API returns filtered results"

**Routing Decision:** Both planners (backend first, then frontend)
**Context Skill(s) to Load First:** Both `saurun:dotnet-tactical-ddd` and `saurun:react-tailwind-v4-components`

**Order of Operations:**
1. Load `saurun:dotnet-tactical-ddd`
2. Invoke `saurun:dotnet-writing-plans` (API filtering logic, query parameter handling)
3. Load `saurun:react-tailwind-v4-components`
4. Invoke `saurun:react-writing-plans` (filter component UI, query param synchronization)

**Reasoning:** Integration signal: end-to-end feature spanning both layers. Backend-first: API contract must be defined before frontend can plan state/UI binding.
