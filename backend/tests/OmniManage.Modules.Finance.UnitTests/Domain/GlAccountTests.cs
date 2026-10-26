using FluentAssertions;
using OmniManage.Modules.Finance.Domain.Errors;
using OmniManage.Modules.Finance.Domain.Gl;

namespace OmniManage.Modules.Finance.UnitTests.Domain;

public sealed class GlAccountTests
{
    private static readonly Guid AccountId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000901");
    private static readonly Guid TenantId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000902");
    private static readonly Guid UserId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000903");
    private static readonly DateTimeOffset Now = new(2026, 10, 22, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WithValidValues_NormalizesCodeAndName()
    {
        var result = GlAccount.Create(
            AccountId,
            TenantId,
            "  1100  ",
            "  Accounts Receivable  ",
            GlAccountType.Asset,
            isActive: true,
            Now,
            UserId);

        result.IsSuccess.Should().BeTrue();

        var account = result.Value;
        account.Id.Should().Be(AccountId);
        account.TenantId.Should().Be(TenantId);
        account.AccountCode.Should().Be("1100");
        account.AccountName.Should().Be("Accounts Receivable");
        account.Type.Should().Be(GlAccountType.Asset);
        account.IsActive.Should().BeTrue();
        account.CreatedAt.Should().Be(Now);
        account.CreatedBy.Should().Be(UserId);
    }

    [Fact]
    public void Create_WithLowercaseCode_UppercasesCode()
    {
        var result = GlAccount.Create(
            AccountId,
            TenantId,
            "ar.control",
            "A/R Control",
            GlAccountType.Asset,
            isActive: true,
            Now,
            UserId);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccountCode.Should().Be("AR.CONTROL");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("-1100")]
    [InlineData("1100/01")]
    [InlineData("123456789012345678901")]
    public void Create_WithInvalidCode_ReturnsValidationError(string code)
    {
        var result = GlAccount.Create(
            AccountId,
            TenantId,
            code,
            "Accounts Receivable",
            GlAccountType.Asset,
            isActive: true,
            Now,
            UserId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(GlAccountErrors.InvalidCode);
    }

    [Fact]
    public void Create_WithTooLongName_ReturnsValidationError()
    {
        var result = GlAccount.Create(
            AccountId,
            TenantId,
            "1100",
            new string('A', GlAccount.NameMaxLength + 1),
            GlAccountType.Asset,
            isActive: true,
            Now,
            UserId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(GlAccountErrors.InvalidName);
    }

    [Fact]
    public void Create_WithUndefinedType_ReturnsValidationError()
    {
        var result = GlAccount.Create(
            AccountId,
            TenantId,
            "1100",
            "Accounts Receivable",
            (GlAccountType)99,
            isActive: true,
            Now,
            UserId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(GlAccountErrors.InvalidType);
    }

    [Fact]
    public void Create_WithEmptyIdentifiers_ReturnsValidationErrors()
    {
        GlAccount.Create(
            Guid.Empty,
            TenantId,
            "1100",
            "Accounts Receivable",
            GlAccountType.Asset,
            isActive: true,
            Now,
            UserId).Error.Should().Be(GlAccountErrors.InvalidId);

        GlAccount.Create(
            AccountId,
            Guid.Empty,
            "1100",
            "Accounts Receivable",
            GlAccountType.Asset,
            isActive: true,
            Now,
            UserId).Error.Should().Be(GlAccountErrors.InvalidTenantId);

        GlAccount.Create(
            AccountId,
            TenantId,
            "1100",
            "Accounts Receivable",
            GlAccountType.Asset,
            isActive: true,
            default,
            UserId).Error.Should().Be(GlAccountErrors.InvalidCreatedAt);

        GlAccount.Create(
            AccountId,
            TenantId,
            "1100",
            "Accounts Receivable",
            GlAccountType.Asset,
            isActive: true,
            Now,
            Guid.Empty).Error.Should().Be(GlAccountErrors.InvalidCreatedBy);
    }
}
