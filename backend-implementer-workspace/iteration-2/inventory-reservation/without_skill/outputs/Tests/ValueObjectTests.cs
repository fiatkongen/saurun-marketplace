using FluentAssertions;
using InventoryReservation.Domain.ValueObjects;
using Xunit;

namespace InventoryReservation.Tests;

public class ValueObjectTests
{
    // --- ProductId ---

    [Fact]
    public void ProductId_Create_GeneratesUniqueIds()
    {
        var id1 = ProductId.Create();
        var id2 = ProductId.Create();
        id1.Should().NotBe(id2);
    }

    [Fact]
    public void ProductId_EmptyGuid_Throws()
    {
        var act = () => ProductId.From(Guid.Empty);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ProductId_SameGuid_AreEqual()
    {
        var guid = Guid.NewGuid();
        ProductId.From(guid).Should().Be(ProductId.From(guid));
    }

    // --- ReservationId ---

    [Fact]
    public void ReservationId_EmptyGuid_Throws()
    {
        var act = () => ReservationId.From(Guid.Empty);
        act.Should().Throw<ArgumentException>();
    }

    // --- Quantity ---

    [Fact]
    public void Quantity_NegativeValue_Throws()
    {
        var act = () => Quantity.Of(-1);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Quantity_Zero_IsValid()
    {
        Quantity.Zero.Value.Should().Be(0);
    }

    [Fact]
    public void Quantity_Addition_Works()
    {
        var result = Quantity.Of(3) + Quantity.Of(5);
        result.Value.Should().Be(8);
    }

    [Fact]
    public void Quantity_Subtraction_Works()
    {
        var result = Quantity.Of(10) - Quantity.Of(3);
        result.Value.Should().Be(7);
    }

    [Fact]
    public void Quantity_Subtraction_GoingNegative_Throws()
    {
        var act = () => Quantity.Of(3) - Quantity.Of(5);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Quantity_Comparison_Works()
    {
        (Quantity.Of(5) > Quantity.Of(3)).Should().BeTrue();
        (Quantity.Of(3) < Quantity.Of(5)).Should().BeTrue();
        (Quantity.Of(3) >= Quantity.Of(3)).Should().BeTrue();
        (Quantity.Of(3) <= Quantity.Of(3)).Should().BeTrue();
    }

    [Fact]
    public void Quantity_Min_ReturnsSmallerValue()
    {
        Quantity.Of(5).Min(Quantity.Of(3)).Value.Should().Be(3);
        Quantity.Of(2).Min(Quantity.Of(7)).Value.Should().Be(2);
    }

    [Fact]
    public void Quantity_Equality_ValueBased()
    {
        Quantity.Of(5).Should().Be(Quantity.Of(5));
        Quantity.Of(5).Should().NotBe(Quantity.Of(3));
    }
}
