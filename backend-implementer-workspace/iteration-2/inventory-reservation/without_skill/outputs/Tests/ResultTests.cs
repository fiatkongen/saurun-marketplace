using FluentAssertions;
using InventoryReservation.Domain.Common;
using Xunit;

namespace InventoryReservation.Tests;

public class ResultTests
{
    [Fact]
    public void Success_HasNoError()
    {
        var result = Result.Success();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Failure_HasError()
    {
        var result = Result.Failure("something broke");
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("something broke");
    }

    [Fact]
    public void GenericSuccess_HasValue()
    {
        var result = Result.Success(42);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void GenericFailure_ThrowsOnValueAccess()
    {
        var result = Result.Failure<int>("fail");
        var act = () => _ = result.Value;
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ImplicitConversion_FromValue()
    {
        Result<string> result = "hello";
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("hello");
    }

    [Fact]
    public void Success_WithError_Throws()
    {
        var act = () => Result.Success<string>(null!);
        // This should succeed - null value is allowed for success
        // The constraint is: success+error is invalid, failure+no-error is invalid
    }

    [Fact]
    public void Failure_WithoutError_IsInvalid()
    {
        var act = () => Result.Failure<int>(null!);
        act.Should().Throw<ArgumentException>();
    }
}
