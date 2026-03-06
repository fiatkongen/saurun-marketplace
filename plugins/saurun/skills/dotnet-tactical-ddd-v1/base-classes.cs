// =============================================================================
// DDD Base Classes - Copy into your Domain/ project
// Lightweight, no external dependencies. Adapt as needed.
// Required: using System; using System.Collections.Generic; using System.Linq;
//           using System.Threading; using System.Threading.Tasks;
// =============================================================================

// --- Result<T> (Railway-oriented error handling) ---
// Use Result (non-generic) for void operations: Result.Success(), Result.Failure("msg")
// Use Result<T> when returning a value: Result.Success<Product>(product), Result.Failure<Product>("msg")

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; }

    protected Result(bool isSuccess, string error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, string.Empty);
    public static Result Failure(string error) => new(false, error);
    public static Result<T> Success<T>(T value) => new(value, true, string.Empty);
    public static Result<T> Failure<T>(string error) => new(default!, false, error);
}

public class Result<T> : Result
{
    public T Value { get; }

    internal Result(T value, bool isSuccess, string error) : base(isSuccess, error)
    {
        Value = value;
    }
}

// --- ValueObject ---

public abstract class ValueObject
{
    protected abstract IEnumerable<object> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType()) return false;
        var other = (ValueObject)obj;
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode() =>
        GetEqualityComponents()
            .Aggregate(0, (hash, component) =>
                HashCode.Combine(hash, component?.GetHashCode() ?? 0));

    public static bool operator ==(ValueObject? left, ValueObject? right) =>
        Equals(left, right);

    public static bool operator !=(ValueObject? left, ValueObject? right) =>
        !Equals(left, right);
}

// --- Entity<TId> ---

public abstract class Entity<TId> where TId : notnull
{
    public TId Id { get; protected set; }

    protected Entity(TId id) => Id = id;

    // EF Core needs parameterless constructor
    protected Entity() => Id = default!;

    public override bool Equals(object? obj) =>
        obj is Entity<TId> other && Id.Equals(other.Id);

    public override int GetHashCode() => Id.GetHashCode();
}

// --- AggregateRoot<TId> ---

public abstract class AggregateRoot<TId> : Entity<TId> where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected AggregateRoot(TId id) : base(id) { }
    protected AggregateRoot() { }

    protected void AddDomainEvent(IDomainEvent domainEvent) =>
        _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}

// --- Domain Events ---

public interface IDomainEvent
{
    DateTime OccurredAt { get; }
}

public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
{
    Task Handle(TEvent domainEvent, CancellationToken ct = default);
}

// --- Domain Event Dispatcher (register in DI) ---

public interface IDomainEventDispatcher
{
    Task DispatchEventsAsync(IEnumerable<IDomainEvent> events, CancellationToken ct = default);
}

// Infrastructure implementation:
// public class DomainEventDispatcher : IDomainEventDispatcher
// {
//     private readonly IServiceProvider _provider;
//     public async Task DispatchEventsAsync(IEnumerable<IDomainEvent> events, CancellationToken ct)
//     {
//         foreach (var @event in events)
//         {
//             var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(@event.GetType());
//             var handlers = _provider.GetServices(handlerType);
//             foreach (dynamic handler in handlers)
//                 await handler.Handle((dynamic)@event, ct);
//         }
//     }
// }
// Usage: await _dispatcher.DispatchEventsAsync(aggregate.DomainEvents, ct);
//        aggregate.ClearDomainEvents();
