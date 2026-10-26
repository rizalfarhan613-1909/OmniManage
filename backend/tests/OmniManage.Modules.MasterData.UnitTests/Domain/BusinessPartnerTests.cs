using FluentAssertions;
using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.MasterData.Domain.BusinessPartners;
using OmniManage.Modules.MasterData.Domain.Errors;

namespace OmniManage.Modules.MasterData.UnitTests.Domain;

public sealed class BusinessPartnerTests
{
    private static readonly Guid Id = Guid.Parse("0192f5b4-8a00-7000-8000-000000000601");
    private static readonly Guid TenantId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000602");
    private static readonly Guid UserId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000603");
    private static readonly DateTimeOffset Now = new(2026, 10, 21, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WithValidValues_NormalizesAndInitializesAggregate()
    {
        var result = BusinessPartner.Create(
            Id,
            TenantId,
            "  bp-001  ",
            "  Acme Customer  ",
            "customer",
            "  1234567890  ",
            1500.5m,
            Now,
            UserId);

        result.IsSuccess.Should().BeTrue();
        var partner = result.Value;
        partner.Id.Should().Be(Id);
        partner.TenantId.Should().Be(TenantId);
        partner.Code.Should().Be("BP-001");
        partner.Name.Should().Be("Acme Customer");
        partner.PartnerType.Should().Be(PartnerType.Customer);
        partner.TaxId.Should().Be("1234567890");
        partner.CreditLimit.Should().Be(1500.5m);
        partner.IsActive.Should().BeTrue();
        partner.CreatedAt.Should().Be(Now);
        partner.CreatedBy.Should().Be(UserId);
        partner.UpdatedAt.Should().BeNull();
        partner.UpdatedBy.Should().BeNull();
    }

    [Fact]
    public void Create_WithBothPartnerType_ParsesType()
    {
        var result = Create(partnerType: "BOTH");

        result.IsSuccess.Should().BeTrue();
        result.Value.PartnerType.Should().Be(PartnerType.Both);
    }

    [Fact]
    public void Create_WhenTaxIdIsBlank_StoresNull()
    {
        var result = Create(taxId: "   ");

        result.IsSuccess.Should().BeTrue();
        result.Value.TaxId.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("A")]
    [InlineData(" BP 1")]
    [InlineData("BP@1")]
    public void Create_WhenCodeIsInvalid_ReturnsInvalidCode(string? code)
    {
        var result = Create(code: code);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(BusinessPartnerErrors.InvalidCode);
    }

    [Fact]
    public void Create_WhenCodeExceedsMaximumLength_ReturnsInvalidCode()
    {
        var result = Create(code: new string('A', BusinessPartner.CodeMaxLength + 1));

        result.Error.Should().Be(BusinessPartnerErrors.InvalidCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WhenNameIsMissing_ReturnsInvalidName(string? name)
    {
        var result = Create(name: name);

        result.Error.Should().Be(BusinessPartnerErrors.InvalidName);
    }

    [Fact]
    public void Create_WhenNameExceedsMaximumLength_ReturnsInvalidName()
    {
        var result = Create(name: new string('N', BusinessPartner.NameMaxLength + 1));

        result.Error.Should().Be(BusinessPartnerErrors.InvalidName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Supplier")]
    [InlineData("1")]
    public void Create_WhenPartnerTypeIsInvalid_ReturnsInvalidPartnerType(string? partnerType)
    {
        var result = Create(partnerType: partnerType);

        result.Error.Should().Be(BusinessPartnerErrors.InvalidPartnerType);
    }

    [Fact]
    public void Create_WhenTaxIdExceedsMaximumLength_ReturnsInvalidTaxId()
    {
        var result = Create(taxId: new string('9', BusinessPartner.TaxIdMaxLength + 1));

        result.Error.Should().Be(BusinessPartnerErrors.InvalidTaxId);
    }

    [Fact]
    public void Create_WhenCreditLimitIsNegative_ReturnsInvalidCreditLimit()
    {
        var result = Create(creditLimit: -0.01m);

        result.Error.Should().Be(BusinessPartnerErrors.InvalidCreditLimit);
    }

    [Fact]
    public void Create_WhenCreditLimitExceedsMaximum_ReturnsInvalidCreditLimit()
    {
        var result = Create(creditLimit: BusinessPartner.CreditLimitMax + 1m);

        result.Error.Should().Be(BusinessPartnerErrors.InvalidCreditLimit);
    }

    [Fact]
    public void Create_WhenIdentifierIsEmpty_ReturnsInvalidId()
    {
        var result = BusinessPartner.Create(
            Guid.Empty,
            TenantId,
            "BP-001",
            "Acme",
            "Customer",
            null,
            0m,
            Now,
            UserId);

        result.Error.Should().Be(BusinessPartnerErrors.InvalidId);
    }

    [Fact]
    public void Create_WhenTenantIsEmpty_ReturnsInvalidTenantId()
    {
        var result = BusinessPartner.Create(
            Id,
            Guid.Empty,
            "BP-001",
            "Acme",
            "Customer",
            null,
            0m,
            Now,
            UserId);

        result.Error.Should().Be(BusinessPartnerErrors.InvalidTenantId);
    }

    [Fact]
    public void Create_WhenCreatedByIsEmpty_ReturnsInvalidCreatedBy()
    {
        var result = BusinessPartner.Create(
            Id,
            TenantId,
            "BP-001",
            "Acme",
            "Customer",
            null,
            0m,
            Now,
            Guid.Empty);

        result.Error.Should().Be(BusinessPartnerErrors.InvalidCreatedBy);
    }

    private static Result<BusinessPartner> Create(
        string? code = "BP-001",
        string? name = "Acme Customer",
        string? partnerType = "Customer",
        string? taxId = null,
        decimal creditLimit = 1000m) =>
        BusinessPartner.Create(Id, TenantId, code, name, partnerType, taxId, creditLimit, Now, UserId);
}
