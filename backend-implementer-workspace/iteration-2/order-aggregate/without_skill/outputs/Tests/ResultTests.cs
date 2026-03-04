using Domain;
using Xunit;

namespace Tests;

public class ResultTests
{
    [Fact]
    public void Success_IsSuccess_True()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Failure_IsFailure_True()
    {
        var result = Result.Failure("something went wrong");

        Assert.True(result.IsFailure);
        Assert.False(result.IsSuccess);
        Assert.Equal("something went wrong", result.Error);
    }

    [Fact]
    public void SuccessT_ContainsValue()
    {
        var result = Result.Success(42);

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void FailureT_AccessingValue_Throws()
    {
        var result = Result.Failure<int>("error");

        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void Map_OnSuccess_TransformsValue()
    {
        var result = Result.Success(5);

        var mapped = result.Map(x => x * 2);

        Assert.True(mapped.IsSuccess);
        Assert.Equal(10, mapped.Value);
    }

    [Fact]
    public void Map_OnFailure_PropagatesError()
    {
        var result = Result.Failure<int>("error");

        var mapped = result.Map(x => x * 2);

        Assert.True(mapped.IsFailure);
        Assert.Equal("error", mapped.Error);
    }

    [Fact]
    public void Bind_OnSuccess_ChainsOperation()
    {
        var result = Result.Success(5);

        var bound = result.Bind(x =>
            x > 0 ? Result.Success(x.ToString()) : Result.Failure<string>("non-positive"));

        Assert.True(bound.IsSuccess);
        Assert.Equal("5", bound.Value);
    }

    [Fact]
    public void Bind_OnFailure_PropagatesError()
    {
        var result = Result.Failure<int>("initial error");

        var bound = result.Bind(x => Result.Success(x.ToString()));

        Assert.True(bound.IsFailure);
        Assert.Equal("initial error", bound.Error);
    }

    [Fact]
    public void Bind_ChainedFailure_ReturnsInnerError()
    {
        var result = Result.Success(-1);

        var bound = result.Bind(x =>
            x > 0 ? Result.Success(x) : Result.Failure<int>("must be positive"));

        Assert.True(bound.IsFailure);
        Assert.Equal("must be positive", bound.Error);
    }
}
