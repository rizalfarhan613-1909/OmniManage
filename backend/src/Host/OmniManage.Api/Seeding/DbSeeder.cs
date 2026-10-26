using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.BuildingBlocks.Domain;
using OmniManage.BuildingBlocks.Infrastructure.Persistence;
using OmniManage.Modules.AssetMaintenance.Infrastructure.Persistence;
using OmniManage.Modules.Finance.Domain.Gl;
using OmniManage.Modules.Finance.Infrastructure.Persistence;
using OmniManage.Modules.Hris.Infrastructure.Persistence;
using OmniManage.Modules.Inventory.Domain.Warehouses;
using OmniManage.Modules.Inventory.Infrastructure.Persistence;
using OmniManage.Modules.MasterData.Domain.BusinessPartners;
using OmniManage.Modules.MasterData.Domain.Items;
using OmniManage.Modules.MasterData.Infrastructure.Persistence;
using OmniManage.Modules.Platform.Application.Auth;
using OmniManage.Modules.Platform.Domain.Identity;
using OmniManage.Modules.Platform.Domain.Tenancy;
using OmniManage.Modules.Platform.Infrastructure.Persistence;
using OmniManage.Modules.Procurement.Domain.Suppliers;
using OmniManage.Modules.Procurement.Infrastructure.Persistence;
using OmniManage.Modules.Projects.Infrastructure.Persistence;

namespace OmniManage.Api.Seeding;

/// <summary>
/// Idempotent developer seeder. It runs at startup only when
/// <c>Seeding:Enabled</c> is true and never runs in Production by default.
/// Each module is seeded through its own DbContext and tenant context, so the
/// same RLS policy that protects runtime traffic also protects the seed.
/// </summary>
public sealed class DbSeeder(
    PlatformDbContext platform,
    IOptions<PostgreSqlOptions> postgreSqlOptions,
    IOptions<SeedingOptions> seedingOptions,
    IPasswordHasher passwordHasher,
    IGuidGenerator guidGenerator,
    TimeProvider clock,
    ILogger<DbSeeder> logger)
{
    /// <summary>
    /// Every module in the catalogue is active for the demo tenant so that each
    /// persona sees its licensed workspace and cross-module read permissions
    /// (for example CRM for sales and A/P for finance) are effective.
    /// </summary>
    private static readonly string[] DefaultModuleCodes =
    [
        ModuleCodes.PlatformCore,
        ModuleCodes.CoreMasterData,
        ModuleCodes.InvCore,
        ModuleCodes.MfgCore,
        ModuleCodes.SalesCore,
        ModuleCodes.CrmCore,
        ModuleCodes.ProcCore,
        ModuleCodes.FinGl,
        ModuleCodes.FinAr,
        ModuleCodes.FinAp,
        ModuleCodes.HrisCore,
        ModuleCodes.HrisAttendance,
        ModuleCodes.HrisPayroll,
        ModuleCodes.HrisEss,
        ModuleCodes.ProjectCore,
        ModuleCodes.AssetCore,
    ];

    private static readonly SampleItem[] SampleItems =
    [
        new("LAPTOP-PRO-14", "ProBook 14 Laptop", "PCS", "Electronics", 1_899.00m, 1_450.00m, true),
        new("CHAIR-ERG-01", "Ergonomic Office Chair", "PCS", "Furniture", 349.00m, 220.00m, true),
        new("CONSULT-HR", "Consulting Services (Hour)", "HRS", "Services", 150.00m, 0m, false),
    ];

    private static readonly SamplePartner[] SamplePartners =
    [
        new("CUST-0001", "Acme Retail Group", PartnerType.Customer, 250_000.00m),
        new("VEND-0001", "Global Supplies Ltd", PartnerType.Vendor, 0m),
    ];

    private static readonly SampleSupplier[] SampleSuppliers =
    [
        new("SUP-0001", "Global Supplies Ltd", "Ayu Lestari", "orders@globalsupplies.test", "+62-21-555-0101", 30),
        new("SUP-0002", "Nusantara Packaging", "Bima Saputra", "sales@nusantara-packaging.test", "+62-21-555-0102", 14),
    ];

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var options = seedingOptions.Value;

        if (!options.Enabled)
        {
            logger.LogDebug("Database seeding is disabled.");
            return;
        }

        var now = clock.GetUtcNow();

        // A fixed seed keeps IDs, names and values stable between local runs.
        Randomizer.Seed = new Random(20261023);

        var (tenant, adminUser) = await SeedPlatformAsync(options, now, cancellationToken)
            .ConfigureAwait(false);

        var customerIds = await SeedMasterDataAsync(tenant.Id, adminUser.Id, now, cancellationToken)
            .ConfigureAwait(false);
        await SeedInventoryAsync(tenant.Id, adminUser.Id, now, cancellationToken)
            .ConfigureAwait(false);
        await SeedProcurementAsync(tenant.Id, adminUser.Id, now, cancellationToken)
            .ConfigureAwait(false);
        await SeedFinanceAsync(tenant.Id, adminUser.Id, now, cancellationToken)
            .ConfigureAwait(false);
        var employeeIds = await SeedHrisAsync(tenant.Id, now, cancellationToken)
            .ConfigureAwait(false);
        await SeedProjectsAsync(tenant.Id, customerIds, employeeIds, now, cancellationToken)
            .ConfigureAwait(false);
        await SeedAssetMaintenanceAsync(tenant.Id, now, cancellationToken)
            .ConfigureAwait(false);

        logger.LogInformation(
            "Demo data is ready for tenant {TenantCode} with admin {AdminEmail}.",
            tenant.Code,
            adminUser.Email);
    }

    private async Task<(Tenant Tenant, User AdminUser)> SeedPlatformAsync(
        SeedingOptions options,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var tenant = await platform.Tenants
            .FirstOrDefaultAsync(item => item.Code == options.DemoTenantCode, cancellationToken)
            .ConfigureAwait(false);

        if (tenant is null)
        {
            tenant = EnsureSuccess(Tenant.Create(
                guidGenerator.NewGuid(),
                options.DemoTenantCode,
                options.DemoTenantName,
                "STANDARD",
                now));

            platform.Tenants.Add(tenant);
            logger.LogInformation("Seeding demo tenant {TenantCode}.", tenant.Code);
        }

        await SeedSuperadminAsync(options, now, cancellationToken).ConfigureAwait(false);

        var permissions = await platform.Permissions.ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var permissionIds = permissions.Select(permission => permission.Id)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var permissionCode in Permissions.All)
        {
            if (permissionIds.Contains(permissionCode))
            {
                continue;
            }

            var permission = EnsureSuccess(Permission.Create(permissionCode));
            platform.Permissions.Add(permission);
            permissions.Add(permission);
        }

        var roles = await platform.Roles
            .Include(role => role.RolePermissions)
            .Where(role => role.TenantId == tenant.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var roleName in SystemRoles.All)
        {
            if (roles.Exists(role => role.Name == roleName))
            {
                continue;
            }

            var role = EnsureSuccess(Role.Create(guidGenerator.NewGuid(), tenant.Id, roleName, isSystem: true));
            platform.Roles.Add(role);
            roles.Add(role);
        }

        var adminRole = roles.Single(role => role.Name == SystemRoles.TenantAdmin);

        foreach (var permission in permissions)
        {
            EnsureSuccess(adminRole.GrantPermission(permission));
        }

        SeedDemoRolePermissions(roles, permissions);

        var tenantUsers = await platform.Users
            .Include(user => user.UserRoles)
            .Where(user => user.TenantId == tenant.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var adminUser = tenantUsers.FirstOrDefault(
            user => string.Equals(user.Email, options.DemoAdminEmail, StringComparison.OrdinalIgnoreCase));

        if (adminUser is null)
        {
            adminUser = EnsureSuccess(User.Create(
                guidGenerator.NewGuid(),
                tenant.Id,
                options.DemoAdminEmail,
                passwordHasher.HashPassword(options.DemoAdminPassword)));

            platform.Users.Add(adminUser);
            tenantUsers.Add(adminUser);
            logger.LogInformation("Seeding demo tenant administrator {AdminEmail}.", adminUser.Email);
        }

        EnsureSuccess(adminUser.AssignRole(adminRole));
        SeedDemoUsers(tenant.Id, tenantUsers, roles, options);

        var licensedModules = await platform.TenantModules
            .Where(module => module.TenantId == tenant.Id)
            .Select(module => module.ModuleCode)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var licensedCodes = licensedModules.ToHashSet(StringComparer.Ordinal);

        foreach (var moduleCode in DefaultModuleCodes)
        {
            if (licensedCodes.Contains(moduleCode))
            {
                continue;
            }

            var tenantModule = EnsureSuccess(TenantModule.CreateActive(
                tenant.Id,
                moduleCode,
                adminUser.Id,
                "Initial demo provisioning",
                now));

            platform.TenantModules.Add(tenantModule);
        }

        await platform.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return (tenant, adminUser);
    }

    private static void SeedDemoRolePermissions(
        IReadOnlyList<Role> roles,
        IReadOnlyList<Permission> permissions)
    {
        var permissionsByCode = permissions.ToDictionary(
            permission => permission.Code,
            StringComparer.Ordinal);

        foreach (var (roleName, permissionCodes) in DemoUserCatalog.PermissionsByRole)
        {
            var role = roles.Single(item => string.Equals(item.Name, roleName, StringComparison.Ordinal));

            foreach (var permissionCode in permissionCodes)
            {
                EnsureSuccess(role.GrantPermission(permissionsByCode[permissionCode]));
            }
        }
    }

    private void SeedDemoUsers(
        Guid tenantId,
        IReadOnlyList<User> tenantUsers,
        IReadOnlyList<Role> roles,
        SeedingOptions options)
    {
        var missingAccounts = DemoUserCatalog.Users
            .Where(spec => !tenantUsers.Any(user => string.Equals(
                user.Email,
                spec.Email,
                StringComparison.OrdinalIgnoreCase)))
            .ToArray();

        if (missingAccounts.Length == 0)
        {
            return;
        }

        var passwordHash = passwordHasher.HashPassword(options.DemoUserPassword);

        foreach (var account in missingAccounts)
        {
            var role = roles.Single(item => string.Equals(
                item.Name,
                account.RoleName,
                StringComparison.Ordinal));
            var user = EnsureSuccess(User.Create(
                guidGenerator.NewGuid(),
                tenantId,
                account.Email,
                passwordHash));

            platform.Users.Add(user);
            EnsureSuccess(user.AssignRole(role));
            logger.LogInformation(
                "Seeding demo user {DemoEmail} with role {RoleName}.",
                user.Email,
                role.Name);
        }
    }

    private async Task SeedSuperadminAsync(
        SeedingOptions options,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var existing = await platform.PlatformUsers
            .FirstOrDefaultAsync(user => user.Email == options.SuperadminEmail, cancellationToken)
            .ConfigureAwait(false);

        if (existing is not null)
        {
            // Transparently migrate a legacy PBKDF2 hash, or refresh Argon2id
            // parameters, when the configured fixture password still matches.
            var outcome = passwordHasher.VerifyHashedPassword(
                existing.PasswordHash,
                options.SuperadminPassword);

            if (outcome == PasswordVerificationOutcome.SuccessRehashNeeded)
            {
                EnsureSuccess(existing.ChangePasswordHash(
                    passwordHasher.HashPassword(options.SuperadminPassword)));
                await platform.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                logger.LogInformation(
                    "Rehashed the System Superadmin password with current Argon2id parameters.");
            }

            return;
        }

        var superadmin = EnsureSuccess(PlatformUser.Create(
            guidGenerator.NewGuid(),
            options.SuperadminEmail,
            passwordHasher.HashPassword(options.SuperadminPassword),
            now));

        platform.PlatformUsers.Add(superadmin);
        logger.LogInformation("Seeding System Superadmin {SuperadminEmail}.", superadmin.Email);
    }

    private async Task<IReadOnlyList<Guid>> SeedMasterDataAsync(
        Guid tenantId,
        Guid createdBy,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var tenant = new SeedTenantContext(tenantId);
        var optionsBuilder = new DbContextOptionsBuilder<MasterDataDbContext>();
        optionsBuilder.UseOmniManagePostgreSql(postgreSqlOptions.Value, MasterDataDbContext.Schema);
        optionsBuilder.AddTenantIsolation(tenant);

        await using var context = new MasterDataDbContext(optionsBuilder.Options, tenant);

        var itemSkus = await context.Items
            .Select(item => item.Sku)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var existingSkus = itemSkus.ToHashSet(StringComparer.Ordinal);

        foreach (var sample in SampleItems)
        {
            if (existingSkus.Contains(sample.Sku))
            {
                continue;
            }

            context.Items.Add(EnsureSuccess(Item.Create(
                guidGenerator.NewGuid(),
                tenantId,
                sample.Sku,
                sample.Name,
                sample.UomCode,
                sample.Category,
                sample.BasePrice,
                sample.CostPrice,
                sample.IsStockTracked,
                now,
                createdBy)));
        }

        var partnerCodes = await context.BusinessPartners
            .Select(partner => partner.Code)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var existingCodes = partnerCodes.ToHashSet(StringComparer.Ordinal);

        foreach (var sample in SamplePartners)
        {
            if (existingCodes.Contains(sample.Code))
            {
                continue;
            }

            context.BusinessPartners.Add(EnsureSuccess(BusinessPartner.Create(
                guidGenerator.NewGuid(),
                tenantId,
                sample.Code,
                sample.Name,
                sample.PartnerType.ToString(),
                taxId: null,
                sample.CreditLimit,
                now,
                createdBy)));
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return await context.BusinessPartners
            .Where(partner => partner.PartnerType == PartnerType.Customer)
            .Select(partner => partner.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task SeedInventoryAsync(
        Guid tenantId,
        Guid createdBy,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var tenant = new SeedTenantContext(tenantId);
        var optionsBuilder = new DbContextOptionsBuilder<InventoryDbContext>();
        optionsBuilder.UseOmniManagePostgreSql(postgreSqlOptions.Value, InventoryDbContext.Schema);
        optionsBuilder.AddTenantIsolation(tenant);

        await using var context = new InventoryDbContext(optionsBuilder.Options, tenant);

        var exists = await context.Warehouses
            .AnyAsync(warehouse => warehouse.Code == "WH-01", cancellationToken)
            .ConfigureAwait(false);

        if (exists)
        {
            return;
        }

        context.Warehouses.Add(EnsureSuccess(Warehouse.Create(
            guidGenerator.NewGuid(),
            tenantId,
            "WH-01",
            "Main Distribution Warehouse",
            "1 Logistics Way, Jakarta",
            now,
            createdBy)));

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task SeedProcurementAsync(
        Guid tenantId,
        Guid createdBy,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var tenant = new SeedTenantContext(tenantId);
        var optionsBuilder = new DbContextOptionsBuilder<ProcurementDbContext>();
        optionsBuilder.UseOmniManagePostgreSql(postgreSqlOptions.Value, ProcurementDbContext.Schema);
        optionsBuilder.AddTenantIsolation(tenant);

        await using var context = new ProcurementDbContext(optionsBuilder.Options, tenant);

        var supplierCodes = await context.Suppliers
            .Select(supplier => supplier.Code)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var existingCodes = supplierCodes.ToHashSet(StringComparer.Ordinal);

        foreach (var sample in SampleSuppliers)
        {
            if (existingCodes.Contains(sample.Code))
            {
                continue;
            }

            context.Suppliers.Add(EnsureSuccess(Supplier.Create(
                guidGenerator.NewGuid(),
                tenantId,
                sample.Code,
                sample.Name,
                sample.ContactName,
                sample.Email,
                sample.Phone,
                sample.PaymentTermsDays,
                now,
                createdBy)));
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task SeedFinanceAsync(
        Guid tenantId,
        Guid createdBy,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var tenant = new SeedTenantContext(tenantId);
        var optionsBuilder = new DbContextOptionsBuilder<FinanceDbContext>();
        optionsBuilder.UseOmniManagePostgreSql(postgreSqlOptions.Value, FinanceDbContext.Schema);
        optionsBuilder.AddTenantIsolation(tenant);

        await using var context = new FinanceDbContext(optionsBuilder.Options, tenant);

        var accountCodes = await context.GlAccounts
            .Select(account => account.AccountCode)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var existingCodes = accountCodes.ToHashSet(StringComparer.Ordinal);

        if (!existingCodes.Contains(ChartOfAccounts.AccountsReceivableCode))
        {
            context.GlAccounts.Add(EnsureSuccess(GlAccount.Create(
                guidGenerator.NewGuid(),
                tenantId,
                ChartOfAccounts.AccountsReceivableCode,
                ChartOfAccounts.AccountsReceivableName,
                GlAccountType.Asset,
                isActive: true,
                now,
                createdBy)));
        }

        if (!existingCodes.Contains(ChartOfAccounts.SalesRevenueCode))
        {
            context.GlAccounts.Add(EnsureSuccess(GlAccount.Create(
                guidGenerator.NewGuid(),
                tenantId,
                ChartOfAccounts.SalesRevenueCode,
                ChartOfAccounts.SalesRevenueName,
                GlAccountType.Revenue,
                isActive: true,
                now,
                createdBy)));
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<IReadOnlyList<Guid>> SeedHrisAsync(
        Guid tenantId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var tenant = new SeedTenantContext(tenantId);
        var optionsBuilder = new DbContextOptionsBuilder<HrisDbContext>();
        optionsBuilder.UseOmniManagePostgreSql(postgreSqlOptions.Value, HrisDbContext.Schema);
        optionsBuilder.AddTenantIsolation(tenant);

        await using var context = new HrisDbContext(optionsBuilder.Options, tenant);

        var employeeIds = await HrisDataSeeder.SeedAsync(
                context,
                tenantId,
                guidGenerator,
                now,
                cancellationToken)
            .ConfigureAwait(false);

        logger.LogInformation(
            "Seeded {EmployeeCount} demo employees with attendance and payroll history.",
            employeeIds.Count);

        return employeeIds;
    }

    private async Task SeedProjectsAsync(
        Guid tenantId,
        IReadOnlyList<Guid> customerIds,
        IReadOnlyList<Guid> employeeIds,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var tenant = new SeedTenantContext(tenantId);
        var optionsBuilder = new DbContextOptionsBuilder<ProjectsDbContext>();
        optionsBuilder.UseOmniManagePostgreSql(postgreSqlOptions.Value, ProjectsDbContext.Schema);
        optionsBuilder.AddTenantIsolation(tenant);

        await using var context = new ProjectsDbContext(optionsBuilder.Options, tenant);

        await ProjectsDataSeeder.SeedAsync(
                context,
                tenantId,
                customerIds,
                employeeIds,
                guidGenerator,
                now,
                cancellationToken)
            .ConfigureAwait(false);

        logger.LogInformation("Seeded demo projects, tasks and work logs.");
    }

    private async Task SeedAssetMaintenanceAsync(
        Guid tenantId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var tenant = new SeedTenantContext(tenantId);
        var optionsBuilder = new DbContextOptionsBuilder<AssetMaintenanceDbContext>();
        optionsBuilder.UseOmniManagePostgreSql(postgreSqlOptions.Value, AssetMaintenanceDbContext.Schema);
        optionsBuilder.AddTenantIsolation(tenant);

        await using var context = new AssetMaintenanceDbContext(optionsBuilder.Options, tenant);

        await AssetMaintenanceDataSeeder.SeedAsync(
                context,
                tenantId,
                guidGenerator,
                now,
                cancellationToken)
            .ConfigureAwait(false);

        logger.LogInformation("Seeded demo assets and maintenance orders.");
    }

    private static T EnsureSuccess<T>(Result<T> result)
    {
        if (result.IsFailure)
        {
            throw new InvalidOperationException(
                $"The demo seed could not be created: {result.Error.Code}.");
        }

        return result.Value;
    }

    private static Result EnsureSuccess(Result result)
    {
        if (result.IsFailure)
        {
            throw new InvalidOperationException(
                $"The demo seed could not be created: {result.Error.Code}.");
        }

        return result;
    }

    private sealed record SampleItem(
        string Sku,
        string Name,
        string UomCode,
        string Category,
        decimal BasePrice,
        decimal CostPrice,
        bool IsStockTracked);

    private sealed record SamplePartner(
        string Code,
        string Name,
        PartnerType PartnerType,
        decimal CreditLimit);

    private sealed record SampleSupplier(
        string Code,
        string Name,
        string? ContactName,
        string? Email,
        string? Phone,
        int PaymentTermsDays);
}
