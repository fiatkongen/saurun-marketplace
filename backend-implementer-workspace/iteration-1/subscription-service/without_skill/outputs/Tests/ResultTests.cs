using SubscriptionManagement.Domain;
using Xunit;

namespace SubscriptionManagement.Tests;

public class ResultTests
{
    [Fact]
    public void Success_Result_Has_Value()
    {
        var result = Result<int>.Success(42);

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Failure_Result_Has_Error()
    {
        var result = Result<int>.Failure("Something went wrong");

        Assert.True(result.IsFailure);
        Assert.False(result.IsSuccess);
        Assert.Equal("Something went wrong", result.Error);
    }

    [Fact]
    public void Accessing_Value_On_Failure_Throws()
    {
        var result = Result<int>.Failure("fail");

        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void Accessing_Error_On_Success_Throws()
    {
        var result = Result<int>.Success(1);

        Assert.Throws<InvalidOperationException>(() => result.Error);
    }

    [Fact]
    public void Implicit_Conversion_Creates_Success()
    {
        Result<string> result = "hello";

        Assert.True(result.IsSuccess);
        Assert.Equal("hello", result.Value);
    }
}
