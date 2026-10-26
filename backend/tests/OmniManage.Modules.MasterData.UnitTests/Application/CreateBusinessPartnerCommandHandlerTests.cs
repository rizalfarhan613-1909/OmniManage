using FluentAssertions;
using OmniManage.Modules.MasterData.Application.BusinessPartners.CreateBusinessPartner;
using OmniManage.Modules.MasterData.Domain.Errors;
using OmniManage.Modules.MasterData.UnitTests.TestDoubles;

namespace OmniManage.Modules.MasterData.UnitTests.Application;

public sealed class CreateBusinessPartnerCommandHandlerTests
{
    private static readonly Guid PartnerId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000631");
    private static readonly Guid TenantId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000632");
    private static readonly Guid UserId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000633");
    private static readonly DateTimeOffset Now = new(2026, 10, 21, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Handle_WithValidCommand_PersistsAndReturnsDto()
    {
        var repository = new FakeBusinessPartnerRepository();
        var handler = CreateHandler(repository);

        var result = await handler.Handle(
            new CreateBusinessPartnerCommand("bp-001", "Acme Customer", "Customer", "1234567890", 2500.75m),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Code.Should().Be("BP-001");
        result.Value.PartnerType.Should().Be("Customer");
        result.Value.TaxIdMasked.Should().Be("****7890");
        result.Value.CreditLimit.Should().Be("2500.7500");

        repository.Partners.Should().ContainSingle();
        repository.Partners[0].TenantId.Should().Be(TenantId);
        repository.Partners[0].CreatedBy.Should().Be(UserId);
        repository.Partners[0].CreatedAt.Should().Be(Now);
        repository.SaveCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WhenCodeAlreadyExists_ReturnsConflictWithoutSaving()
    {
        var repository = new FakeBusinessPartnerRepository { CodeExists = true };
        var handler = CreateHandler(repository);

        var result = await handler.Handle(
            new CreateBusinessPartnerCommand("BP-001", "Acme Customer", "Customer", null, 0m),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(BusinessPartnerErrors.CodeAlreadyExists);
        repository.Partners.Should().BeEmpty();
        repository.SaveCount.Should().Be(0);
    }

    private static CreateBusinessPartnerCommandHandler CreateHandler(FakeBusinessPartnerRepository repository) =>
        new(
            repository,
            new TestTenantContext(TenantId),
            new FakeCurrentUser(UserId),
            new FixedGuidGenerator(PartnerId),
            new FixedTimeProvider(Now));
}
