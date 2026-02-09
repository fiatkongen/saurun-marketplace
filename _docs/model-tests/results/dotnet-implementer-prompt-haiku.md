I'll generate a properly structured dispatch prompt following the dotnet-implementer-prompt pattern you provided.

```
Task tool (saurun:backend-implementer):
  description: "Implement CreateOrder Command Handler"
  prompt: |
    You are implementing the CreateOrder Command Handler for the Orders bounded context.

    ## Task Description

    Implement `CreateOrderCommandHandler` in `Application/Orders/Commands/`.

    The command contains:
    - `CustomerId` (required)
    - `List<OrderItem>` with `ProductId` (required) and `Quantity` (required)
    - `ShippingAddress` (required)

    Handler must:
    - Validate: customer exists, all products exist, quantities > 0, max 100 items per order
    - Create Order aggregate from the command
    - Add items to the order
    - Calculate order total
    - Save via `IOrderRepository`
    - Return `OrderId` and `Total`
    - Return 400 if validation fails (bad quantities, too many items)
    - Return 404 if customer or any product not found

    ## Context

    This is in the ShoppingApp.Backend Orders bounded context, Application layer.
    The Domain layer already has an `Order` aggregate and `OrderItem` entity.
    Follow the existing pattern from `CreateCustomerCommandHandler` for command
    and handler structure. This is a Carter API project using CQRS pattern.

    No other tasks depend on this. This task has no external dependencies.

    ## Before You Begin

    If the task description above is ambiguous or missing acceptance criteria,
    ask for clarification before starting. Otherwise, proceed immediately.

    ## Your Job

    1. Implement exactly what the task specifies
    2. **REQUIRED SUB-SKILL:** Follow `saurun:dotnet-tdd` strictly. No exceptions.
    3. Verify implementation works: `dotnet test`
    4. Commit your work
    5. Self-review (see below)
    6. Report back

    Work from: /Users/dev/shopping-app/backend

    While you work: if you encounter something unexpected or unclear, ask
    questions. Don't guess or make assumptions.

    ## Before Reporting Back: Self-Review

    Review your work with fresh eyes. Ask yourself:

    **Completeness:**
    - Did I fully implement everything in the spec?
    - Did I miss any requirements (400 and 404 cases, all validations)?
    - Are there edge cases I didn't handle (empty items list, zero total, etc.)?

    **Quality:**
    - Is this my best work?
    - Are names clear and accurate (match what things do, not how they work)?
    - Is the code clean and maintainable?

    **Discipline:**
    - Did I avoid overbuilding (YAGNI)?
    - Did I only build what was requested?
    - Did I follow existing patterns (CQRS handlers, Carter conventions)?

    **Testing:**
    - Did I follow TDD? (test first, watch fail, minimal code, watch pass, refactor)
    - Would each test catch a real bug? If not, delete it.
    - REFERENCE: See `saurun:dotnet-tdd` verification checklist for complete test quality criteria.

    If you find issues during self-review, fix them now before reporting.

    ## Report Format

    When done, report:
    - What you implemented
    - What you tested and test results
    - Files changed
    - Self-review findings (if any)
    - Any issues or concerns
```

This prompt:
- **Includes full task spec** (subagent can't read your plan file)
- **Sets architectural context** (Orders bounded context, Application layer, CQRS pattern, existing Order aggregate)
- **Specifies working directory** (absolute path)
- **Mandates TDD via saurun:dotnet-tdd** (Red-Green-Refactor cycle)
- **Includes complete self-review checklist** (completeness, quality, discipline, testing)
- **Defines structured report format** (so you can parse results)
- **No ambiguity** (all error codes, validation rules, return values explicit)

Ready to dispatch to `saurun:backend-implementer`.