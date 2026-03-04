# Booking System MVP - Transcript

## Task

Build a booking system MVP: customers book time slots, each slot has date/start time/end time/max capacity. Customers can book if capacity remains.

## Approach

Followed `dotnet-tactical-ddd` skill strictly with `dotnet-tdd` TDD workflow. Despite "speed > architecture" pressure, applied all patterns: value objects, aggregate root with private constructor + static factory returning Result<T>, domain events, private collections, behavior methods enforcing invariants.

## TDD Cycles

### Cycle 1: TimeSlot Value Object

**RED** - Wrote 4 tests in `Tests/TimeSlotTests.cs`:
- `Create_WithValidTimes_ReturnsSuccess` - happy path, verify all 3 properties
- `Create_WithEndBeforeStart_ReturnsFailure` - invalid: end < start
- `Create_WithEqualStartAndEnd_ReturnsFailure` - invalid: zero-duration slot
- `Equals_WithSameValues_ReturnsTrue` - value equality

Tests failed: `The type or namespace name 'ValueObjects' does not exist` (correct RED - class missing).

**GREEN** - Implemented `Domain/ValueObjects/TimeSlot.cs`:
- Private constructor, static `Create()` factory returning `Result<TimeSlot>`
- Validates end > start
- Extends `ValueObject` with equality on Date, StartTime, EndTime

Result: 4/4 passed.

### Cycle 2: Capacity Value Object

**RED** - Wrote 3 tests (one `[Theory]` with 3 `[InlineData]`) in `Tests/CapacityTests.cs`:
- `Create_WithPositiveValue_ReturnsSuccess`
- `Create_WithZeroOrNegativeValue_ReturnsFailure` (0, -1, -100)
- `Equals_WithSameValue_ReturnsTrue`

Tests failed: `The name 'Capacity' does not exist` (correct RED).

**GREEN** - Implemented `Domain/ValueObjects/Capacity.cs`:
- Private constructor, `Create()` factory returning `Result<Capacity>`
- Validates maxCapacity > 0

Result: 9/9 passed.

### Cycle 3: BookableSlot Aggregate + Booking Entity

**RED** - Wrote 10 tests in `Tests/BookableSlotTests.cs`:
- `Create_WithValidInputs_ReturnsSuccessWithCorrectProperties` - factory happy path
- `Create_RaisesSlotCreatedEvent` - domain event on creation
- `Book_WithCapacityAvailable_ReturnsSuccess` - booking happy path
- `Book_RecordsCustomerId` - verify customer ID stored correctly
- `Book_WhenAtCapacity_ReturnsFailure` - capacity invariant enforcement
- `Book_MultipleCustomers_UntilFull_Works` - fill to capacity, then fail
- `Book_WithEmptyCustomerId_ReturnsFailure` - input validation
- `Book_SameCustomerTwice_ReturnsFailure` - duplicate booking prevention
- `Book_RaisesBookingCreatedEvent` - domain event on booking
- `RemainingCapacity_ReflectsBookings` - computed property accuracy

Tests failed: `The type or namespace name 'Entities' does not exist` (correct RED).

**GREEN** - Implemented 4 files:
- `Domain/Events/SlotCreatedEvent.cs` - immutable record, past tense
- `Domain/Events/BookingCreatedEvent.cs` - immutable record, past tense
- `Domain/Entities/Booking.cs` - entity with private setters, internal constructor (only aggregate creates bookings)
- `Domain/Entities/BookableSlot.cs` - aggregate root with:
  - Private constructor + `Create()` factory returning `Result<BookableSlot>`
  - Private `_bookings` list with `IReadOnlyList` projection
  - `Book(Guid customerId)` behavior method enforcing all invariants
  - `RemainingCapacity` computed property
  - Domain events on create and book

Result: 19/19 passed.

## DDD Patterns Applied

| Pattern | Implementation |
|---------|---------------|
| Value Objects | `TimeSlot` (date+start+end), `Capacity` (max slots) |
| Entity | `Booking` (identity, private setters) |
| Aggregate Root | `BookableSlot` (transaction boundary, invariant enforcement) |
| Static Factory + Result<T> | All domain types use `Create()` returning `Result<T>`, private constructors |
| Domain Events | `SlotCreatedEvent`, `BookingCreatedEvent` (immutable records, past tense) |
| Private Collections | `_bookings` list with `IReadOnlyList<Booking>` projection |
| Behavior Methods | `Book()` encapsulates all booking logic and validation |

## Invariants Enforced by Domain

1. Time slot end must be after start (no zero-duration or reversed slots)
2. Capacity must be positive
3. Cannot book with empty customer ID
4. Cannot exceed slot capacity
5. Same customer cannot book the same slot twice

## Final Test Count

**19 tests, all passing** (4 TimeSlot + 5 Capacity + 10 BookableSlot)

## Files Written

### Domain/
- `Domain/Common/Result.cs` - Railway-oriented error handling
- `Domain/Common/ValueObject.cs` - Value equality base class
- `Domain/Common/Entity.cs` - Identity-based equality base class
- `Domain/Common/AggregateRoot.cs` - Domain event support base class
- `Domain/Common/IDomainEvent.cs` - Domain event interface
- `Domain/ValueObjects/TimeSlot.cs` - Date + start/end time value object
- `Domain/ValueObjects/Capacity.cs` - Max capacity value object
- `Domain/Entities/BookableSlot.cs` - Aggregate root with booking behavior
- `Domain/Entities/Booking.cs` - Booking entity
- `Domain/Events/SlotCreatedEvent.cs` - Slot creation domain event
- `Domain/Events/BookingCreatedEvent.cs` - Booking creation domain event
- `Domain/Domain.csproj` - Project file

### Tests/
- `Tests/TimeSlotTests.cs` - 4 tests
- `Tests/CapacityTests.cs` - 5 tests (incl. Theory)
- `Tests/BookableSlotTests.cs` - 10 tests
- `Tests/GlobalUsings.cs` - Xunit global using
- `Tests/Tests.csproj` - Test project file

### Root
- `Booking.sln` - Solution file
