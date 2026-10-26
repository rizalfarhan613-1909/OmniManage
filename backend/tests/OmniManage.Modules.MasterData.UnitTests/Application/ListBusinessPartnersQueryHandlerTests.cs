using FluentAssertions;
using OmniManage.Modules.MasterData.Application.BusinessPartners.ListBusinessPartners;
using OmniManage.Modules.MasterData.Domain.Abstractions;
using OmniManage.Modules.MasterData.Domain.BusinessPartners;
using OmniManage.Modules.MasterData.UnitTests.TestDoubles;

namespace OmniManage.Modules.MasterData.UnitTests.Application;

public sealed class ListBusinessPartnersQueryHandlerTests
{
    private static readonly Guid TenantId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000651");
    private static readonly Guid UserId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000652");
    private static readonly DateTimeOffset Now = new(2026, 10, 21, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Handle_ClampsPaginationAndParsesSort()
    {
        var repository = new FakeBusinessPartnerRepository();
        repository.Partners.Add(CreatePartner(1, "BP-001", "Acme One"));
        repository.Partners.Add(CreatePartner(2, "BP-002", "Acme Two"));

        var handler = new ListBusinessPartnersQueryHandler(repository);

        var result = await handler.Handle(
            new ListBusinessPartnersQuery(Page: 0, PageSize: 500, Search: "  acme  ", Sort: "-name"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Page.Should().Be(1);
        result.Value.PageSize.Should().Be(100);
        result.Value.TotalItems.Should().Be(2);
        result.Value.Items.Should().HaveCount(2);

        repository.LastCriteria.Should().NotBeNull();
        repository.LastCriteria!.Skip.Should().Be(0);
        repository.LastCriteria.Take.Should().Be(100);
        repository.LastCriteria.Search.Should().Be("acme");
        repository.LastCriteria.Sort.Should().ContainSingle();

        var sort = repository.LastCriteria.Sort[0];
        sort.Field.Should().Be(BusinessPartnerSortField.Name);
        sort.Descending.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithUnknownSortField_FallsBackToCodeAscending()
    {
        var repository = new FakeBusinessPartnerRepository();
        var handler = new ListBusinessPartnersQueryHandler(repository);

        var result = await handler.Handle(
            new ListBusinessPartnersQuery(Page: 2, PageSize: 10, Sort: "taxId"),
            CancellationToken.None);

        result.Value.Page.Should().Be(2);
        repository.LastCriteria!.Skip.Should().Be(10);
        repository.LastCriteria.Sort[0].Field.Should().Be(BusinessPartnerSortField.Code);
        repository.LastCriteria.Sort[0].Descending.Should().BeFalse();
    }

    private static BusinessPartner CreatePartner(int sequence, string code, string name) =>
        BusinessPartner.Create(
            Guid.Parse($"0192f5b4-8a00-7000-8000-{sequence:D12}"),
            TenantId,
            code,
            name,
            "Customer",
            null,
            0m,
            Now,
            UserId).Value;
}
