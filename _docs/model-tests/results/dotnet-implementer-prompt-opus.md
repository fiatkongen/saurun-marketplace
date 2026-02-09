

Here is the dispatch prompt for Task 2:

---

**Task tool (`saurun:backend-implementer`):**
**Description:** "Implement CreateOrder command handler"

**Prompt:**

You are implementing the CreateOrder command handler for the Orders API.

## Task Description

Implement `CreateOrderCommandHandler` in `Application/Orders/Commands/`. The command `CreateOrderCommand` contains:
- `CustomerId` (Guid, required)
- `Items` (List of `OrderItemDto` with `ProductId` (Guid) and `Quantity` (int))
- `ShippingAddress` (string, required)

The handler should:
- Validate that the customer exists (via `ICustomerRepository`) -- return 404 if not found
- Validate that all products exist (via `IProductRepository`) -- return 404 if any product not found
- Validate that all quantities are > 0 -- return 400 if any fail
- Validate max 100 items per order -- return 400 if exceeded
- Create an `Order` aggregate, add items, calculate total (using product prices from the repository)
- Save the order via `IOrderRepository`
- Return `OrderId` and `Total` on success

Error responses:
- 400 if validation fails (quantity <= 0, items exceed 100, missing shipping address)
- 404 if customer not found or any product not found

## Context

This is in the **Orders** bounded context of `ShoppingApp.Backend`. The Domain layer already has an `Order` aggregate with an `Items` collection and an `OrderItem` entity. The Application layer has `IOrderRepository`, `ICustomerRepository`, and `IProductRepository` interfaces. Follow the existing pattern from `CreateCustomerCommandHandler` for the command/handler structure. The API layer uses Carter for minimal API endpoints -- follow the existing endpoint patterns for the POST endpoint.

No other tasks depend on this, and this task has no dependencies on other tasks.

## Before You Begin

If the task description above is ambiguous or missing acceptance criteria, ask for clarification before starting. Otherwise, proceed immediately.

## Your Job

1. Implement exactly what the task specifies
2. **REQUIRED SUB-SKILL:** Follow `saurun:dotnet-tdd` strictly. No exceptions.
3. Verify implementation works: `dotnet test`
4. Commit your work
5. Self-review (see below)
6. Report back

Work from: `/Users/dev/shopping-app/backend`

While you work: if you encounter something unexpected or unclear, ask questions. Don't guess or make assumptions.

## Before Reporting Back: Self-Review

Review your work with fresh eyes. Ask yourself:

**Completeness:**
- Did I fully implement everything in the spec?
- Did I miss any requirements (404 for customer, 404 for products, 400 for validation, total calculation)?
- Are there edge cases I didn't handle (empty items list, duplicate product IDs, empty shipping address)?

**Quality:**
- Is this my best work?
- Are names clear and accurate?
- Is the code clean and maintainable?

**Discipline:**
- Did I avoid overbuilding (YAGNI)?
- Did I only build what was requested?
- Did I follow existing patterns (Carter endpoints, CQRS handlers, `CreateCustomerCommandHandler` structure)?

**Testing:**
- Did I follow TDD? (test first, watch fail, minimal code, watch pass, refactor)
- Would each test catch a real bug? If not, delete it.
- **REFERENCE:** See `saurun:dotnet-tdd` verification checklist for complete test quality criteria.

If you find issues during self-review, fix them now before reporting.

## Report Format

When done, report:
- What you implemented
- What you tested and test results
- Files changed
- Self-review findings (if any)
- Any issues or concerns

---

**What this prompt covers:**

- Full task spec pasted inline -- subagent never needs to read a plan file
- Bounded context, layer, existing entities, existing patterns, and repository interfaces all named explicitly
- All error cases enumerated (400, 404) with specific triggers
- Working directory specified as absolute path
- TDD enforced via `saurun:dotnet-tdd` sub-skill reference
- Self-review checklist tailored to this task's specific requirements (customer lookup, product lookup, total calculation, item limit)
- Clean report format for orchestrator consumption
- No inter-task dependencies to worry about