using FluentAssertions;
using OmniManage.BuildingBlocks.Domain;

namespace OmniManage.BuildingBlocks.UnitTests;

public sealed class EntityTests
{
    [Fact]
    public void Equals_WhenSameTypeAndId_ReturnsTrue()
    {
        var id = Guid.Parse("0192f5b4-8a00-7000-8000-000000000001");

        var left = new TestEntity(id);
        var right = new TestEntity(id);

        left.Should().Be(right);
        (left == right).Should().BeTrue();
        left.GetHashCode().Should().Be(right.GetHashCode());
    }

    [Fact]
    public void Equals_WhenDifferentIds_ReturnsFalse()
    {
        var left = new TestEntity(Guid.Parse("0192f5b4-8a00-7000-8000-000000000001"));
        var right = new TestEntity(Guid.Parse("0192f5b4-8a00-7000-8000-000000000002"));

        left.Should().NotBe(right);
        (left != right).Should().BeTrue();
    }

    [Fact]
    public void Equals_WhenBothTransient_ReturnsFalse()
    {
        var left = new TestEntity();
        var right = new TestEntity();

        left.Should().NotBe(right);
    }

    private sealed class TestEntity : Entity<Guid>
    {
        public TestEntity()
        {
        }

        public TestEntity(Guid id)
            : base(id)
        {
        }
    }
}
