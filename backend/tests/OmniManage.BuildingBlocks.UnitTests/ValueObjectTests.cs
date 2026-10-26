using FluentAssertions;
using OmniManage.BuildingBlocks.Domain;

namespace OmniManage.BuildingBlocks.UnitTests;

public sealed class ValueObjectTests
{
    [Fact]
    public void Equals_WhenAllComponentsMatch_ReturnsTrue()
    {
        var left = new Money(1250000.00m, "IDR");
        var right = new Money(1250000.00m, "IDR");

        left.Should().Be(right);
        (left == right).Should().BeTrue();
        left.GetHashCode().Should().Be(right.GetHashCode());
    }

    [Fact]
    public void Equals_WhenAnyComponentDiffers_ReturnsFalse()
    {
        var left = new Money(1250000.00m, "IDR");
        var right = new Money(1250000.00m, "USD");

        left.Should().NotBe(right);
        (left != right).Should().BeTrue();
    }

    [Fact]
    public void Equals_WhenDifferentValueObjectTypes_ReturnsFalse()
    {
        var money = new Money(100m, "IDR");
        var quantity = new Quantity(100m, "IDR");

        money.Should().NotBe(quantity);
    }

    private sealed class Money(decimal amount, string currency) : ValueObject
    {
        public decimal Amount { get; } = amount;

        public string Currency { get; } = currency;

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Amount;
            yield return Currency;
        }
    }

    private sealed class Quantity(decimal amount, string uom) : ValueObject
    {
        public decimal Amount { get; } = amount;

        public string Uom { get; } = uom;

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Amount;
            yield return Uom;
        }
    }
}
