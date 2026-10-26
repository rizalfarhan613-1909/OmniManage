using FluentAssertions;
using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Platform.Domain.Errors;
using OmniManage.Modules.Platform.Domain.Tenancy;

namespace OmniManage.Modules.Platform.UnitTests.Tenancy;

public sealed class TenantModuleTests
{
    private static readonly Guid TenantId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000201");
    private static readonly Guid ChangedBy = Guid.Parse("0192f5b4-8a00-7000-8000-000000000202");
    private static readonly DateTimeOffset Now = new(2026, 10, 21, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CreateActive_WithValidData_ReturnsEffectiveLicense()
    {
        var result = TenantModule.CreateActive(
            TenantId,
            "hris_payroll",
            ChangedBy,
            "Initial subscription",
            Now,
            Now.AddMonths(12));

        result.IsSuccess.Should().BeTrue();
        result.Value.TenantId.Should().Be(TenantId);
        result.Value.ModuleCode.Should().Be("HRIS_PAYROLL");
        result.Value.IsActive.Should().BeTrue();
        result.Value.ActivatedAt.Should().Be(Now);
        result.Value.ExpiresAt.Should().Be(Now.AddMonths(12));
        result.Value.ChangedBy.Should().Be(ChangedBy);
        result.Value.ChangeReason.Should().Be("Initial subscription");
        result.Value.Version.Should().Be(1);
        result.Value.IsEffectiveAt(Now).Should().BeTrue();
    }

    [Fact]
    public void CreateInactive_WithValidData_StartsInactiveWithoutActivationTime()
    {
        var result = TenantModule.CreateInactive(TenantId, "FIN_GL", ChangedBy, "Provisioned");

        result.IsSuccess.Should().BeTrue();
        result.Value.IsActive.Should().BeFalse();
        result.Value.ActivatedAt.Should().BeNull();
        result.Value.ExpiresAt.Should().BeNull();
        result.Value.Version.Should().Be(1);
    }

    [Fact]
    public void CreateActive_WhenTenantIdIsEmpty_ReturnsInvalidTenantId()
    {
        var result = TenantModule.CreateActive(Guid.Empty, "FIN_GL", ChangedBy, "Grant", Now);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TenantModuleErrors.InvalidTenantId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("FI")]
    [InlineData("FIN-GL")]
    [InlineData("FIN GL")]
    [InlineData("_FIN")]
    public void CreateActive_WhenModuleCodeIsInvalid_ReturnsInvalidModuleCode(string moduleCode)
    {
        var result = TenantModule.CreateActive(TenantId, moduleCode, ChangedBy, "Grant", Now);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TenantModuleErrors.InvalidModuleCode);
    }

    [Fact]
    public void CreateActive_WhenChangedByIsEmpty_ReturnsInvalidChangedBy()
    {
        var result = TenantModule.CreateActive(TenantId, "FIN_GL", Guid.Empty, "Grant", Now);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TenantModuleErrors.InvalidChangedBy);
    }

    [Fact]
    public void CreateActive_WhenReasonIsBlank_ReturnsChangeReasonRequired()
    {
        var result = TenantModule.CreateActive(TenantId, "FIN_GL", ChangedBy, "  ", Now);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TenantModuleErrors.ChangeReasonRequired);
    }

    [Fact]
    public void CreateActive_WhenReasonIsTooLong_ReturnsChangeReasonTooLong()
    {
        var result = TenantModule.CreateActive(
            TenantId,
            "FIN_GL",
            ChangedBy,
            new string('x', TenantModule.ChangeReasonMaxLength + 1),
            Now);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TenantModuleErrors.ChangeReasonTooLong);
    }

    [Fact]
    public void CreateActive_WhenActivationTimeIsDefault_ReturnsInvalidActivationTime()
    {
        var result = TenantModule.CreateActive(TenantId, "FIN_GL", ChangedBy, "Grant", default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TenantModuleErrors.InvalidActivationTime);
    }

    [Fact]
    public void CreateActive_WhenExpiryIsNotInTheFuture_ReturnsInvalidExpiry()
    {
        var result = TenantModule.CreateActive(
            TenantId,
            "FIN_GL",
            ChangedBy,
            "Grant",
            Now,
            Now);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TenantModuleErrors.InvalidExpiry);
    }

    [Fact]
    public void Activate_WhenInactive_ActivatesAndRecordsChange()
    {
        var tenantModule = TenantModule.CreateInactive(TenantId, "FIN_GL", ChangedBy, "Provisioned").Value;
        var activatedAt = Now.AddHours(1);

        var result = tenantModule.Activate(ChangedBy, "Purchased", activatedAt, activatedAt.AddYears(1));

        result.IsSuccess.Should().BeTrue();
        tenantModule.IsActive.Should().BeTrue();
        tenantModule.ActivatedAt.Should().Be(activatedAt);
        tenantModule.ExpiresAt.Should().Be(activatedAt.AddYears(1));
        tenantModule.ChangeReason.Should().Be("Purchased");
        tenantModule.Version.Should().Be(2);
    }

    [Fact]
    public void Activate_WhenAlreadyEffective_ReturnsAlreadyActive()
    {
        var tenantModule = TenantModule.CreateActive(TenantId, "FIN_GL", ChangedBy, "Grant", Now).Value;

        var result = tenantModule.Activate(ChangedBy, "Renew", Now.AddMinutes(1));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TenantModuleErrors.AlreadyActive);
        tenantModule.Version.Should().Be(1);
    }

    [Fact]
    public void Activate_WhenCurrentLicenseHasExpired_Reactivates()
    {
        var tenantModule = TenantModule
            .CreateActive(TenantId, "FIN_GL", ChangedBy, "Trial", Now, Now.AddDays(1))
            .Value;
        var renewedAt = Now.AddDays(2);

        var result = tenantModule.Activate(ChangedBy, "Renewal", renewedAt, renewedAt.AddYears(1));

        result.IsSuccess.Should().BeTrue();
        tenantModule.IsActive.Should().BeTrue();
        tenantModule.ActivatedAt.Should().Be(renewedAt);
        tenantModule.IsEffectiveAt(renewedAt).Should().BeTrue();
    }

    [Fact]
    public void Deactivate_WhenActive_DeactivatesAndRecordsChange()
    {
        var tenantModule = TenantModule.CreateActive(TenantId, "FIN_GL", ChangedBy, "Grant", Now).Value;

        var result = tenantModule.Deactivate(ChangedBy, "Contract ended");

        result.IsSuccess.Should().BeTrue();
        tenantModule.IsActive.Should().BeFalse();
        tenantModule.ChangeReason.Should().Be("Contract ended");
        tenantModule.Version.Should().Be(2);
    }

    [Fact]
    public void Deactivate_WhenInactive_ReturnsAlreadyInactive()
    {
        var tenantModule = TenantModule.CreateInactive(TenantId, "FIN_GL", ChangedBy, "Provisioned").Value;

        var result = tenantModule.Deactivate(ChangedBy, "Contract ended");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TenantModuleErrors.AlreadyInactive);
    }

    [Fact]
    public void ChangeExpiry_WhenActive_UpdatesExpiryAndRecordsChange()
    {
        var tenantModule = TenantModule.CreateActive(TenantId, "FIN_GL", ChangedBy, "Grant", Now).Value;
        var newExpiry = Now.AddYears(2);

        var result = tenantModule.ChangeExpiry(ChangedBy, "Extended term", Now.AddMonths(1), newExpiry);

        result.IsSuccess.Should().BeTrue();
        tenantModule.ExpiresAt.Should().Be(newExpiry);
        tenantModule.ChangeReason.Should().Be("Extended term");
        tenantModule.Version.Should().Be(2);
    }

    [Fact]
    public void ChangeExpiry_WhenInactive_ReturnsNotActive()
    {
        var tenantModule = TenantModule.CreateInactive(TenantId, "FIN_GL", ChangedBy, "Provisioned").Value;

        var result = tenantModule.ChangeExpiry(ChangedBy, "Extended term", Now, Now.AddYears(1));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TenantModuleErrors.NotActive);
    }

    [Fact]
    public void ChangeExpiry_WhenExpiryIsInThePast_ReturnsInvalidExpiry()
    {
        var tenantModule = TenantModule.CreateActive(TenantId, "FIN_GL", ChangedBy, "Grant", Now).Value;

        var result = tenantModule.ChangeExpiry(ChangedBy, "Extended term", Now, Now.AddDays(-1));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TenantModuleErrors.InvalidExpiry);
    }

    [Fact]
    public void IsEffectiveAt_WhenActiveAndNotExpired_ReturnsTrue()
    {
        var tenantModule = TenantModule
            .CreateActive(TenantId, "FIN_GL", ChangedBy, "Grant", Now, Now.AddDays(1))
            .Value;

        tenantModule.IsEffectiveAt(Now.AddHours(12)).Should().BeTrue();
    }

    [Fact]
    public void IsEffectiveAt_WhenExpiryEqualsNow_ReturnsFalse()
    {
        var expiresAt = Now.AddDays(1);
        var tenantModule = TenantModule
            .CreateActive(TenantId, "FIN_GL", ChangedBy, "Grant", Now, expiresAt)
            .Value;

        tenantModule.IsEffectiveAt(expiresAt).Should().BeFalse();
    }

    [Fact]
    public void IsEffectiveAt_WhenInactive_ReturnsFalse()
    {
        var tenantModule = TenantModule.CreateInactive(TenantId, "FIN_GL", ChangedBy, "Provisioned").Value;

        tenantModule.IsEffectiveAt(Now).Should().BeFalse();
    }

    [Fact]
    public void TenantModule_ImplementsTenantEntity()
    {
        var tenantModule = TenantModule.CreateInactive(TenantId, "FIN_GL", ChangedBy, "Provisioned").Value;

        tenantModule.Should().BeAssignableTo<ITenantEntity>();
        ((ITenantEntity)tenantModule).TenantId.Should().Be(TenantId);
    }
}
