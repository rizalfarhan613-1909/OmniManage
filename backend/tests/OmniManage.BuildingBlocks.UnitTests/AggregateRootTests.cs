using FluentAssertions;
using OmniManage.BuildingBlocks.Domain;

namespace OmniManage.BuildingBlocks.UnitTests;

public sealed class AggregateRootTests
{
    [Fact]
    public void Raise_WhenCalled_AddsDomainEvent()
    {
        var aggregate = new TestAggregate(Guid.Parse("0192f5b4-8a00-7000-8000-000000000010"));
        var domainEvent = new TestDomainEvent(
            Guid.Parse("0192f5b4-8a00-7000-8000-000000000011"),
            new DateTimeOffset(2026, 10, 21, 8, 15, 30, TimeSpan.Zero));

        aggregate.DoSomething(domainEvent);

        aggregate.DomainEvents.Should().ContainSingle().Which.Should().BeSameAs(domainEvent);
    }

    [Fact]
    public void ClearDomainEvents_WhenCalled_RemovesAllEvents()
    {
        var aggregate = new TestAggregate(Guid.Parse("0192f5b4-8a00-7000-8000-000000000012"));
        var domainEvent = new TestDomainEvent(
            Guid.Parse("0192f5b4-8a00-7000-8000-000000000013"),
            new DateTimeOffset(2026, 10, 21, 8, 15, 30, TimeSpan.Zero));

        aggregate.DoSomething(domainEvent);
        aggregate.ClearDomainEvents();

        aggregate.DomainEvents.Should().BeEmpty();
    }

    private sealed class TestAggregate(Guid id) : AggregateRoot<Guid>(id)
    {
        public void DoSomething(IDomainEvent domainEvent) => Raise(domainEvent);
    }

    private sealed record TestDomainEvent(Guid EventId, DateTimeOffset OccurredOnUtc) : IDomainEvent;
}
