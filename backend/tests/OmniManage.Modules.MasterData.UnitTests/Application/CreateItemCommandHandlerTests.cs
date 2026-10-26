using FluentAssertions;
using OmniManage.Modules.MasterData.Application.Items.CreateItem;
using OmniManage.Modules.MasterData.Domain.Errors;
using OmniManage.Modules.MasterData.UnitTests.TestDoubles;

namespace OmniManage.Modules.MasterData.UnitTests.Application;

public sealed class CreateItemCommandHandlerTests
{
    private static readonly Guid ItemId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000641");
    private static readonly Guid TenantId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000642");
    private static readonly Guid UserId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000643");
    private static readonly DateTimeOffset Now = new(2026, 10, 21, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Handle_WithValidCommand_PersistsAndReturnsDto()
    {
        var repository = new FakeItemRepository();
        var handler = CreateHandler(repository);

        var result = await handler.Handle(
            new CreateItemCommand("sku-001", "Widget", "pcs", "Hardware", 25.5m, 18m, true),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Sku.Should().Be("SKU-001");
        result.Value.UomCode.Should().Be("PCS");
        result.Value.BasePrice.Should().Be("25.5000");
        result.Value.CostPrice.Should().Be("18.0000");

        repository.Items.Should().ContainSingle();
        repository.Items[0].TenantId.Should().Be(TenantId);
        repository.Items[0].CreatedBy.Should().Be(UserId);
        repository.SaveCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WhenSkuAlreadyExists_ReturnsConflictWithoutSaving()
    {
        var repository = new FakeItemRepository { SkuExists = true };
        var handler = CreateHandler(repository);

        var result = await handler.Handle(
            new CreateItemCommand("SKU-001", "Widget", "PCS", null, 10m, 5m, true),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ItemErrors.SkuAlreadyExists);
        repository.Items.Should().BeEmpty();
        repository.SaveCount.Should().Be(0);
    }

    private static CreateItemCommandHandler CreateHandler(FakeItemRepository repository) =>
        new(
            repository,
            new TestTenantContext(TenantId),
            new FakeCurrentUser(UserId),
            new FixedGuidGenerator(ItemId),
            new FixedTimeProvider(Now));
}
