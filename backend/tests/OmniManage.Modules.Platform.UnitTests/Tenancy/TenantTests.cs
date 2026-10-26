using FluentAssertions;
using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Platform.Domain.Errors;
using OmniManage.Modules.Platform.Domain.Tenancy;

namespace OmniManage.Modules.Platform.UnitTests.Tenancy;

public sealed class TenantTests
{
    private static readonly Guid TenantId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000201");
    private static readonly DateTimeOffset CreatedAt = new(2026, 10, 21, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WithValidData_ReturnsActiveTenant()
    {
        var result = Tenant.Create(TenantId, " acme ", " Acme Corporation ", " starter ", CreatedAt);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(TenantId);
        result.Value.Code.Should().Be("ACME");
        result.Value.Name.Should().Be("Acme Corporation");
        result.Value.PlanCode.Should().Be("STARTER");
        result.Value.Status.Should().Be(TenantStatus.Active);
        result.Value.CreatedAt.Should().Be(CreatedAt);
    }

    [Fact]
    public void Create_WhenIdIsEmpty_ReturnsInvalidId()
    {
        var result = Tenant.Create(Guid.Empty, "ACME", "Acme", "STARTER", CreatedAt);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TenantErrors.InvalidId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("A")]
    [InlineData("ACME CORP")]
    [InlineData("ACME!")]
    [InlineData("-ACME")]
    [InlineData("_ACME")]
    public void Create_WhenCodeIsInvalid_ReturnsInvalidCode(string code)
    {
        var result = Tenant.Create(TenantId, code, "Acme", "STARTER", CreatedAt);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TenantErrors.InvalidCode);
    }

    [Fact]
    public void Create_WhenNameIsBlank_ReturnsInvalidName()
    {
        var result = Tenant.Create(TenantId, "ACME", "  ", "STARTER", CreatedAt);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TenantErrors.InvalidName);
    }

    [Fact]
    public void Create_WhenPlanCodeIsBlank_ReturnsInvalidPlanCode()
    {
        var result = Tenant.Create(TenantId, "ACME", "Acme", " ", CreatedAt);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TenantErrors.InvalidPlanCode);
    }

    [Fact]
    public void Create_WhenCreatedAtIsDefault_ReturnsInvalidCreatedAt()
    {
        var result = Tenant.Create(TenantId, "ACME", "Acme", "STARTER", default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TenantErrors.InvalidCreatedAt);
    }

    [Fact]
    public void Suspend_WhenActive_TransitionsToSuspended()
    {
        var tenant = CreateTenant();

        var result = tenant.Suspend();

        result.IsSuccess.Should().BeTrue();
        tenant.Status.Should().Be(TenantStatus.Suspended);
    }

    [Fact]
    public void Suspend_WhenAlreadySuspended_ReturnsConflict()
    {
        var tenant = CreateTenant();
        tenant.Suspend();

        var result = tenant.Suspend();

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Conflict);
        tenant.Status.Should().Be(TenantStatus.Suspended);
    }

    [Fact]
    public void Reactivate_WhenSuspended_TransitionsToActive()
    {
        var tenant = CreateTenant();
        tenant.Suspend();

        var result = tenant.Reactivate();

        result.IsSuccess.Should().BeTrue();
        tenant.Status.Should().Be(TenantStatus.Active);
    }

    [Fact]
    public void Reactivate_WhenActive_ReturnsConflict()
    {
        var tenant = CreateTenant();

        var result = tenant.Reactivate();

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public void Reactivate_WhenArchived_ReturnsConflict()
    {
        var tenant = CreateTenant();
        tenant.Archive();

        var result = tenant.Reactivate();

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Conflict);
        tenant.Status.Should().Be(TenantStatus.Archived);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Archive_WhenNotArchived_TransitionsToArchived(bool suspendFirst)
    {
        var tenant = CreateTenant();

        if (suspendFirst)
        {
            tenant.Suspend();
        }

        var result = tenant.Archive();

        result.IsSuccess.Should().BeTrue();
        tenant.Status.Should().Be(TenantStatus.Archived);
    }

    [Fact]
    public void Archive_WhenAlreadyArchived_ReturnsConflict()
    {
        var tenant = CreateTenant();
        tenant.Archive();

        var result = tenant.Archive();

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public void Suspend_WhenArchived_ReturnsConflict()
    {
        var tenant = CreateTenant();
        tenant.Archive();

        var result = tenant.Suspend();

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Conflict);
        tenant.Status.Should().Be(TenantStatus.Archived);
    }

    private static Tenant CreateTenant() =>
        Tenant.Create(TenantId, "ACME", "Acme Corporation", "STARTER", CreatedAt).Value;
}
