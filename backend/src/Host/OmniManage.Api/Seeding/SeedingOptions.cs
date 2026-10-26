namespace OmniManage.Api.Seeding;

/// <summary>
/// Developer-only seed settings. Seeding is disabled unless
/// <see cref="Enabled"/> is explicitly turned on, which is only the case in
/// <c>appsettings.Development.json</c>.
/// </summary>
public sealed class SeedingOptions
{
    public const string SectionName = "Seeding";

    public bool Enabled { get; init; }

    public string SuperadminEmail { get; init; } = "superadmin@omnimanage.io";

    public string SuperadminPassword { get; init; } = string.Empty;

    public string DemoTenantCode { get; init; } = "TEN-8492";

    public string DemoTenantName { get; init; } = "Acme Global Corp";

    public string DemoAdminEmail { get; init; } = "sarah.jenkins@acme.com";

    public string DemoAdminPassword { get; init; } = string.Empty;

    /// <summary>
    /// Shared password for the demo tenant users created for each system role.
    /// </summary>
    public string DemoUserPassword { get; init; } = "Password123!";
}
