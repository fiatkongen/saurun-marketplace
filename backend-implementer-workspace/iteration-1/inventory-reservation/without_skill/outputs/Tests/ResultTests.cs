using FluentAssertions;
using InventoryReservation.Domain.Common;
using Xunit;

namespace InventoryReservation.Tests;

public class ResultTests
{
    [Fact]
    public void Success_result_has_no_error()
    {
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Failure_result_has_error()
    {
        var result = Result.Failure("something went wrong");

        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("something went wrong");
    }

    [Fact]
    public void Success_generic_carries_value()
    {
        var result = Result.Success(42);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Failure_generic_throws_on_value_access()
    {
        var result = Result.Failure<int>("nope");

        result.IsFailure.Should().BeTrue();
        var act = () => result.Value;
        act.Should().Throw<InvalidOperationException>();
    }
}
