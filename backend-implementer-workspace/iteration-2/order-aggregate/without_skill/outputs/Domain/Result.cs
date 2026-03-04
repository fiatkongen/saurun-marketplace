namespace Domain;

/// <summary>
/// Represents the result of an operation that can succeed or fail without throwing exceptions.
/// </summary>
public class Result
{
    protected Result(bool isSuccess, string? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }

    public static Result Success() => new(true, null);
    public static Result Failure(string error) => new(false, error);
    public static Result<T> Success<T>(T value) => new(value, true, null);
    public static Result<T> Failure<T>(string error) => new(default, false, error);
}

/// <summary>
/// Represents the result of an operation that produces a value of type T.
/// </summary>
public class Result<T> : Result
{
    private readonly T? _value;

    internal Result(T? value, bool isSuccess, string? error) : base(isSuccess, error)
    {
        _value = value;
    }

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access Value on a failed result.");

    public Result<TOut> Map<TOut>(Func<T, TOut> mapper)
    {
        return IsSuccess
            ? Result.Success(mapper(Value))
            : Result.Failure<TOut>(Error!);
    }

    public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> binder)
    {
        return IsSuccess ? binder(Value) : Result.Failure<TOut>(Error!);
    }
}
