using FluentAssertions;
using OmniManage.Modules.MasterData.Application.Items.ListItems;
using OmniManage.Modules.MasterData.Domain.Abstractions;
using OmniManage.Modules.MasterData.UnitTests.TestDoubles;

namespace OmniManage.Modules.MasterData.UnitTests.Application;

public sealed class ListItemsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ComputesSkipAndDefaultsToSkuSort()
    {
        var repository = new FakeItemRepository();
        var handler = new ListItemsQueryHandler(repository);

        var result = await handler.Handle(
            new ListItemsQuery(Page: 2, PageSize: 10, Search: " widget ", Sort: "unknown"),
            CancellationToken.None);

        result.Value.Page.Should().Be(2);
        result.Value.PageSize.Should().Be(10);

        repository.LastCriteria.Should().NotBeNull();
        repository.LastCriteria!.Skip.Should().Be(10);
        repository.LastCriteria.Take.Should().Be(10);
        repository.LastCriteria.Search.Should().Be("widget");
        repository.LastCriteria.Sort[0].Field.Should().Be(ItemSortField.Sku);
        repository.LastCriteria.Sort[0].Descending.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithPriceSort_MapsAllowListedField()
    {
        var repository = new FakeItemRepository();
        var handler = new ListItemsQueryHandler(repository);

        await handler.Handle(
            new ListItemsQuery(Page: 1, PageSize: 25, Sort: "-basePrice"),
            CancellationToken.None);

        repository.LastCriteria!.Sort[0].Field.Should().Be(ItemSortField.BasePrice);
        repository.LastCriteria.Sort[0].Descending.Should().BeTrue();
    }
}
