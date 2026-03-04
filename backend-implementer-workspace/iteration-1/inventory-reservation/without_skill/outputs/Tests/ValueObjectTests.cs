using FluentAssertions;
using InventoryReservation.Domain.ValueObjects;
using Xunit;

namespace InventoryReservation.Tests;

public class ValueObjectTests
{
    [Fact]
    public void ProductId_cannot_be_empty_guid()
    {
        var act = () => new ProductId(Guid.Empty);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ProductId_Create_generates_unique_ids()
    {
        var id1 = ProductId.Create();
        var id2 = ProductId.Create();
        id1.Should().NotBe(id2);
    }

    [Fact]
    public void ProductId_equality_by_value()
    {
        var guid = Guid.NewGuid();
        var id1 = ProductId.From(guid);
        var id2 = ProductId.From(guid);
        id1.Should().Be(id2);
    }

    [Fact]
    public void ReservationId_cannot_be_empty_guid()
    {
        var act = () => new ReservationId(Guid.Empty);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Quantity_cannot_be_negative()
    {
        var act = () => new Quantity(-1);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Quantity_zero_is_valid()
    {
        var q = Quantity.Zero;
        q.Value.Should().Be(0);
    }

    [Fact]
    public void Quantity_arithmetic_works()
    {
        var a = Quantity.Of(10);
        var b = Quantity.Of(3);

        (a + b).Value.Should().Be(13);
        (a - b).Value.Should().Be(7);
    }

    [Fact]
    public void Quantity_subtraction_below_zero_throws()
    {
        var a = Quantity.Of(3);
        var b = Quantity.Of(5);

        var act = () => a - b;
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Quantity_comparison_operators()
    {
        var small = Quantity.Of(2);
        var large = Quantity.Of(10);

        (small < large).Should().BeTrue();
        (large > small).Should().BeTrue();
        (small >= Quantity.Of(2)).Should().BeTrue();
        (small <= Quantity.Of(2)).Should().BeTrue();
    }

    [Fact]
    public void Quantity_equality_by_value()
    {
        Quantity.Of(5).Should().Be(Quantity.Of(5));
        Quantity.Of(5).Should().NotBe(Quantity.Of(3));
    }
}
