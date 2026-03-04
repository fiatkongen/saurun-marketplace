# Transcript: Quick MVP Booking System (without skill)

## Task
Build a minimal booking system: customers book time slots with date, start/end time, max capacity. Book if capacity remains.

## Approach
Speed-first. Two domain entities, one test file. No over-engineering.

## Steps

1. **Created `Domain/TimeSlot.cs`** — Aggregate root. Holds date, start time, end time, max capacity. Owns a list of bookings. `Book(customerName)` method enforces capacity and validates input. `RemainingCapacity` computed property.

2. **Created `Domain/Booking.cs`** — Value-ish entity. Records who booked which slot and when. Created through `TimeSlot.Book()` only.

3. **Created `Tests/TimeSlotTests.cs`** — 10 tests covering:
   - New slot has full capacity
   - Booking reduces remaining capacity
   - Booking returns correct data
   - Cannot book when full (capacity enforced)
   - Multiple bookings up to capacity succeed
   - Empty/whitespace customer name rejected
   - End time must be after start time
   - Equal start/end time rejected
   - Zero capacity rejected
   - Negative capacity rejected

## Design Decisions
- **All booking logic on TimeSlot aggregate** — `Book()` is the single entry point. Capacity check + booking creation are atomic within the aggregate. No way to create a booking that bypasses capacity validation.
- **Private parameterless constructors** — EF Core compatibility without exposing unsafe construction.
- **No repository/service layer** — MVP. Domain entities are self-contained and testable without infrastructure.
- **`IReadOnlyList<Booking>`** — Bookings collection exposed as read-only to prevent external mutation.

## Files Written
- `outputs/Domain/TimeSlot.cs`
- `outputs/Domain/Booking.cs`
- `outputs/Tests/TimeSlotTests.cs`
