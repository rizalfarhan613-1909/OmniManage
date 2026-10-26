using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OmniManage.Api.IntegrationTests.Authorization;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.Modules.AssetMaintenance.Infrastructure.Persistence;
using OmniManage.Modules.Finance.Domain.Ar;
using OmniManage.Modules.Finance.Domain.Gl;
using OmniManage.Modules.Finance.Infrastructure.Persistence;
using OmniManage.Modules.Hris.Domain.Employees;
using OmniManage.Modules.Hris.Infrastructure.Persistence;
using OmniManage.Modules.Inventory.Domain.Warehouses;
using OmniManage.Modules.Inventory.Infrastructure.Persistence;
using OmniManage.Modules.MasterData.Domain.BusinessPartners;
using OmniManage.Modules.MasterData.Domain.Items;
using OmniManage.Modules.MasterData.Infrastructure.Persistence;
using OmniManage.Modules.Manufacturing.Domain.BillOfMaterials;
using OmniManage.Modules.Manufacturing.Infrastructure.Persistence;
using OmniManage.Modules.Platform.Application.Auth;
using OmniManage.Modules.Platform.Domain.Identity;
using OmniManage.Modules.Platform.Domain.Tenancy;
using OmniManage.Modules.Platform.Infrastructure.Persistence;
using OmniManage.Modules.Procurement.Domain.Suppliers;
using OmniManage.Modules.Procurement.Infrastructure.Persistence;
using OmniManage.Modules.Projects.Domain.Projects;
using OmniManage.Modules.Projects.Infrastructure.Persistence;
using OmniManage.Modules.Sales.Domain.Orders;
using OmniManage.Modules.Sales.Infrastructure.Persistence;

namespace OmniManage.Api.IntegrationTests;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    public const string TenantCode = "ACME";
    public const string Email = "admin@acme.test";
    public const string LimitedEmail = "viewer@acme.test";
    public const string BetaTenantCode = "BETA";
    public const string BetaEmail = "admin@beta.test";
    public const string Password = "CorrectHorseBatteryStaple!";

    public static readonly Guid TenantId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000501");
    public static readonly Guid UserId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000502");
    public static readonly Guid RoleId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000503");
    public static readonly Guid LimitedUserId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000504");
    public static readonly Guid LimitedRoleId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000505");
    public static readonly Guid BetaTenantId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000511");
    public static readonly Guid BetaUserId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000512");
    public static readonly Guid BetaRoleId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000513");

    public const string BetaBusinessPartnerCode = "BETA-0001";
    public const string BetaItemSku = "BETA-ITEM-1";
    public const string BetaWarehouseCode = "BETA-WH-1";
    public const string AcmeWarehouseCode = "ACME-WH-1";
    public const string BetaArInvoiceNumber = "AR-BETA-0001";
    public const string BetaJournalEntryNumber = "JE-BETA-0001";
    public const string AcmeSupplierCode = "SUP-ACME-1";
    public const string BetaSupplierCode = "SUP-BETA-1";
    public const string AcmeBomCode = "BOM-ACME-1";
    public const string BetaBomCode = "BOM-BETA-1";

    public static readonly Guid AcmeWarehouseId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000531");
    public static readonly Guid BetaSalesOrderId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000532");
    public static readonly Guid BetaSalesOrderLineId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000533");
    public static readonly Guid BetaSalesOrderItemId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000534");
    public static readonly Guid BetaSalesOrderCustomerId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000535");
    public static readonly Guid BetaArInvoiceId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000536");
    public static readonly Guid BetaJournalEntryId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000537");
    public static readonly Guid BetaReceivableAccountId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000538");
    public static readonly Guid BetaRevenueAccountId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000539");
    public static readonly Guid AcmeEmployeeId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000540");
    public static readonly Guid AcmeDepartmentId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000541");
    public static readonly Guid AcmeProjectId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000542");
    public static readonly Guid AcmeProjectCustomerId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000543");
    public static readonly Guid AcmeSupplierId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000544");
    public static readonly Guid BetaSupplierId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000545");
    public static readonly Guid AcmeFinishedGoodItemId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000546");
    public static readonly Guid AcmeBomComponentItemId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000547");
    public static readonly Guid AcmeBomComponentId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000548");
    public static readonly Guid AcmeBomId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000549");
    public static readonly Guid BetaFinishedGoodItemId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000550");
    public static readonly Guid BetaBomComponentItemId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000551");
    public static readonly Guid BetaBomComponentId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000552");
    public static readonly Guid BetaBomId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000553");

    private readonly string _databaseName = $"omnimanage-api-{Guid.NewGuid():N}";
    private readonly string _masterDataDatabaseName = $"omnimanage-masterdata-api-{Guid.NewGuid():N}";
    private readonly string _inventoryDatabaseName = $"omnimanage-inventory-api-{Guid.NewGuid():N}";
    private readonly string _salesDatabaseName = $"omnimanage-sales-api-{Guid.NewGuid():N}";
    private readonly string _financeDatabaseName = $"omnimanage-finance-api-{Guid.NewGuid():N}";
    private readonly string _hrisDatabaseName = $"omnimanage-hris-api-{Guid.NewGuid():N}";
    private readonly string _projectsDatabaseName = $"omnimanage-projects-api-{Guid.NewGuid():N}";
    private readonly string _assetMaintenanceDatabaseName = $"omnimanage-asset-maintenance-api-{Guid.NewGuid():N}";
    private readonly string _procurementDatabaseName = $"omnimanage-procurement-api-{Guid.NewGuid():N}";
    private readonly string _manufacturingDatabaseName = $"omnimanage-manufacturing-api-{Guid.NewGuid():N}";

    public ApiFactory()
    {
        using var scope = Services.CreateScope();

        Seed(scope.ServiceProvider);
    }

    private string KeyId { get; } = TestJwtKey.CreateKeyId();

    private string PrivateKeyPem { get; } = TestJwtKey.CreatePrivateKeyPem();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = TestJwtKey.Issuer,
                ["Jwt:Audience"] = TestJwtKey.Audience,
                ["Jwt:KeyId"] = KeyId,
                ["Jwt:PrivateKeyPem"] = PrivateKeyPem,
                ["Jwt:AccessTokenMinutes"] = "15",
            });
        });
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<PlatformDbContext>>();
            services.RemoveAll<PlatformDbContext>();
            services.AddDbContext<PlatformDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));

            services.RemoveAll<DbContextOptions<MasterDataDbContext>>();
            services.RemoveAll<MasterDataDbContext>();
            services.AddDbContext<MasterDataDbContext>(options =>
                options.UseInMemoryDatabase(_masterDataDatabaseName));

            services.RemoveAll<DbContextOptions<InventoryDbContext>>();
            services.RemoveAll<InventoryDbContext>();
            services.AddDbContext<InventoryDbContext>(options =>
                options.UseInMemoryDatabase(_inventoryDatabaseName));

            services.RemoveAll<DbContextOptions<SalesDbContext>>();
            services.RemoveAll<SalesDbContext>();
            services.AddDbContext<SalesDbContext>(options =>
                options.UseInMemoryDatabase(_salesDatabaseName));

            services.RemoveAll<DbContextOptions<FinanceDbContext>>();
            services.RemoveAll<FinanceDbContext>();
            services.AddDbContext<FinanceDbContext>(options =>
                options.UseInMemoryDatabase(_financeDatabaseName));

            services.RemoveAll<DbContextOptions<HrisDbContext>>();
            services.RemoveAll<HrisDbContext>();
            services.AddDbContext<HrisDbContext>(options =>
                options.UseInMemoryDatabase(_hrisDatabaseName));

            services.RemoveAll<DbContextOptions<ProjectsDbContext>>();
            services.RemoveAll<ProjectsDbContext>();
            services.AddDbContext<ProjectsDbContext>(options =>
                options.UseInMemoryDatabase(_projectsDatabaseName));

            services.RemoveAll<DbContextOptions<AssetMaintenanceDbContext>>();
            services.RemoveAll<AssetMaintenanceDbContext>();
            services.AddDbContext<AssetMaintenanceDbContext>(options =>
                options.UseInMemoryDatabase(_assetMaintenanceDatabaseName));

            services.RemoveAll<DbContextOptions<ProcurementDbContext>>();
            services.RemoveAll<ProcurementDbContext>();
            services.AddDbContext<ProcurementDbContext>(options =>
                options.UseInMemoryDatabase(_procurementDatabaseName));

            services.RemoveAll<DbContextOptions<ManufacturingDbContext>>();
            services.RemoveAll<ManufacturingDbContext>();
            services.AddDbContext<ManufacturingDbContext>(options =>
                options.UseInMemoryDatabase(_manufacturingDatabaseName));

            services.AddControllers().AddApplicationPart(typeof(PermissionTestController).Assembly);
        });
    }

    private static void Seed(IServiceProvider services)
    {
        var dbContext = services.GetRequiredService<PlatformDbContext>();
        var masterData = services.GetRequiredService<MasterDataDbContext>();
        var inventory = services.GetRequiredService<InventoryDbContext>();
        var sales = services.GetRequiredService<SalesDbContext>();
        var finance = services.GetRequiredService<FinanceDbContext>();
        var hris = services.GetRequiredService<HrisDbContext>();
        var projects = services.GetRequiredService<ProjectsDbContext>();
        var procurement = services.GetRequiredService<ProcurementDbContext>();
        var manufacturing = services.GetRequiredService<ManufacturingDbContext>();
        var passwordHasher = services.GetRequiredService<IPasswordHasher>();
        var now = new DateTimeOffset(2026, 10, 21, 8, 0, 0, TimeSpan.Zero);
        var passwordHash = passwordHasher.HashPassword(Password);

        var tenant = Tenant.Create(TenantId, TenantCode, "Acme Corporation", "STARTER", now).Value;
        var user = User.Create(UserId, TenantId, Email, passwordHash).Value;
        var role = Role.Create(RoleId, TenantId, SystemRoles.TenantAdmin, isSystem: true).Value;

        var salesRead = Permission.Create(Permissions.Sales.OrderRead).Value;
        var salesCreate = Permission.Create(Permissions.Sales.OrderCreate).Value;
        var salesConfirm = Permission.Create(Permissions.Sales.OrderConfirm).Value;
        var businessPartnerRead = Permission.Create(Permissions.MasterData.BusinessPartnerRead).Value;
        var businessPartnerCreate = Permission.Create(Permissions.MasterData.BusinessPartnerCreate).Value;
        var itemRead = Permission.Create(Permissions.MasterData.ItemRead).Value;
        var itemCreate = Permission.Create(Permissions.MasterData.ItemCreate).Value;
        var warehouseRead = Permission.Create(Permissions.Inventory.WarehouseRead).Value;
        var warehouseCreate = Permission.Create(Permissions.Inventory.WarehouseCreate).Value;
        var stockRead = Permission.Create(Permissions.Inventory.StockRead).Value;
        var stockAdjust = Permission.Create(Permissions.Inventory.StockAdjust).Value;
        var invoiceRead = Permission.Create(Permissions.Finance.InvoiceRead).Value;
        var journalRead = Permission.Create(Permissions.Finance.JournalRead).Value;
        var attendanceClockIn = Permission.Create(Permissions.Hris.AttendanceClockIn).Value;
        var payrollApprove = Permission.Create(Permissions.Hris.PayrollApprove).Value;
        var taskCreate = Permission.Create(Permissions.Projects.TaskCreate).Value;
        var workLogCreate = Permission.Create(Permissions.Projects.WorkLogCreate).Value;
        var assetCreate = Permission.Create(Permissions.Assets.AssetCreate).Value;
        var maintenanceRequest = Permission.Create(Permissions.Assets.MaintenanceRequest).Value;
        var supplierRead = Permission.Create(Permissions.Procurement.SupplierRead).Value;
        var purchaseOrderRead = Permission.Create(Permissions.Procurement.PurchaseOrderRead).Value;
        var purchaseOrderCreate = Permission.Create(Permissions.Procurement.PurchaseOrderCreate).Value;
        var workOrderRead = Permission.Create(Permissions.Manufacturing.WorkOrderRead).Value;
        var workOrderCreate = Permission.Create(Permissions.Manufacturing.WorkOrderCreate).Value;
        var billOfMaterialRead = Permission.Create(Permissions.Manufacturing.BillOfMaterialRead).Value;

        user.AssignRole(role);
        role.GrantPermission(salesRead);
        role.GrantPermission(salesCreate);
        role.GrantPermission(salesConfirm);
        role.GrantPermission(businessPartnerRead);
        role.GrantPermission(businessPartnerCreate);
        role.GrantPermission(itemRead);
        role.GrantPermission(itemCreate);
        role.GrantPermission(warehouseRead);
        role.GrantPermission(warehouseCreate);
        role.GrantPermission(stockRead);
        role.GrantPermission(stockAdjust);
        role.GrantPermission(invoiceRead);
        role.GrantPermission(journalRead);
        role.GrantPermission(attendanceClockIn);
        role.GrantPermission(payrollApprove);
        role.GrantPermission(taskCreate);
        role.GrantPermission(workLogCreate);
        role.GrantPermission(assetCreate);
        role.GrantPermission(maintenanceRequest);
        role.GrantPermission(supplierRead);
        role.GrantPermission(purchaseOrderRead);
        role.GrantPermission(purchaseOrderCreate);
        role.GrantPermission(workOrderRead);
        role.GrantPermission(workOrderCreate);
        role.GrantPermission(billOfMaterialRead);

        var limitedUser = User.Create(LimitedUserId, TenantId, LimitedEmail, passwordHash).Value;
        var limitedRole = Role.Create(LimitedRoleId, TenantId, "Viewer", isSystem: false).Value;

        limitedUser.AssignRole(limitedRole);
        limitedRole.GrantPermission(salesRead);

        var betaTenant = Tenant.Create(BetaTenantId, BetaTenantCode, "Beta Industries", "STARTER", now).Value;
        var betaUser = User.Create(BetaUserId, BetaTenantId, BetaEmail, passwordHash).Value;
        var betaRole = Role.Create(BetaRoleId, BetaTenantId, SystemRoles.TenantAdmin, isSystem: true).Value;

        betaUser.AssignRole(betaRole);
        betaRole.GrantPermission(salesRead);
        betaRole.GrantPermission(salesCreate);
        betaRole.GrantPermission(salesConfirm);
        betaRole.GrantPermission(businessPartnerRead);
        betaRole.GrantPermission(businessPartnerCreate);
        betaRole.GrantPermission(itemRead);
        betaRole.GrantPermission(itemCreate);
        betaRole.GrantPermission(warehouseRead);
        betaRole.GrantPermission(warehouseCreate);
        betaRole.GrantPermission(stockRead);
        betaRole.GrantPermission(stockAdjust);
        betaRole.GrantPermission(invoiceRead);
        betaRole.GrantPermission(journalRead);
        betaRole.GrantPermission(attendanceClockIn);
        betaRole.GrantPermission(payrollApprove);
        betaRole.GrantPermission(taskCreate);
        betaRole.GrantPermission(workLogCreate);
        betaRole.GrantPermission(assetCreate);
        betaRole.GrantPermission(maintenanceRequest);
        betaRole.GrantPermission(supplierRead);
        betaRole.GrantPermission(purchaseOrderRead);
        betaRole.GrantPermission(purchaseOrderCreate);
        betaRole.GrantPermission(workOrderRead);
        betaRole.GrantPermission(workOrderCreate);
        betaRole.GrantPermission(billOfMaterialRead);

        dbContext.Tenants.Add(tenant);
        dbContext.Tenants.Add(betaTenant);
        dbContext.Users.Add(user);
        dbContext.Users.Add(limitedUser);
        dbContext.Users.Add(betaUser);
        dbContext.Roles.Add(role);
        dbContext.Roles.Add(limitedRole);
        dbContext.Roles.Add(betaRole);
        dbContext.Permissions.Add(salesRead);
        dbContext.Permissions.Add(salesCreate);
        dbContext.Permissions.Add(salesConfirm);
        dbContext.Permissions.Add(businessPartnerRead);
        dbContext.Permissions.Add(businessPartnerCreate);
        dbContext.Permissions.Add(itemRead);
        dbContext.Permissions.Add(itemCreate);
        dbContext.Permissions.Add(warehouseRead);
        dbContext.Permissions.Add(warehouseCreate);
        dbContext.Permissions.Add(stockRead);
        dbContext.Permissions.Add(stockAdjust);
        dbContext.Permissions.Add(invoiceRead);
        dbContext.Permissions.Add(journalRead);
        dbContext.Permissions.Add(attendanceClockIn);
        dbContext.Permissions.Add(payrollApprove);
        dbContext.Permissions.Add(taskCreate);
        dbContext.Permissions.Add(workLogCreate);
        dbContext.Permissions.Add(assetCreate);
        dbContext.Permissions.Add(maintenanceRequest);
        dbContext.Permissions.Add(supplierRead);
        dbContext.Permissions.Add(purchaseOrderRead);
        dbContext.Permissions.Add(purchaseOrderCreate);
        dbContext.Permissions.Add(workOrderRead);
        dbContext.Permissions.Add(workOrderCreate);
        dbContext.Permissions.Add(billOfMaterialRead);
        dbContext.TenantModules.Add(
            TenantModule.CreateActive(TenantId, ModuleCodes.SalesCore, UserId, "Integration test", now).Value);
        dbContext.TenantModules.Add(
            TenantModule.CreateActive(TenantId, ModuleCodes.CoreMasterData, UserId, "Integration test", now).Value);
        dbContext.TenantModules.Add(
            TenantModule.CreateActive(TenantId, ModuleCodes.InvCore, UserId, "Integration test", now).Value);
        dbContext.TenantModules.Add(
            TenantModule.CreateActive(TenantId, ModuleCodes.FinGl, UserId, "Integration test", now).Value);
        dbContext.TenantModules.Add(
            TenantModule.CreateActive(TenantId, ModuleCodes.FinAr, UserId, "Integration test", now).Value);
        dbContext.TenantModules.Add(
            TenantModule.CreateActive(TenantId, ModuleCodes.HrisAttendance, UserId, "Integration test", now).Value);
        dbContext.TenantModules.Add(
            TenantModule.CreateActive(TenantId, ModuleCodes.HrisPayroll, UserId, "Integration test", now).Value);
        dbContext.TenantModules.Add(
            TenantModule.CreateActive(TenantId, ModuleCodes.ProjectCore, UserId, "Integration test", now).Value);
        dbContext.TenantModules.Add(
            TenantModule.CreateActive(TenantId, ModuleCodes.AssetCore, UserId, "Integration test", now).Value);
        dbContext.TenantModules.Add(
            TenantModule.CreateActive(TenantId, ModuleCodes.ProcCore, UserId, "Integration test", now).Value);
        dbContext.TenantModules.Add(
            TenantModule.CreateActive(TenantId, ModuleCodes.MfgCore, UserId, "Integration test", now).Value);
        dbContext.TenantModules.Add(
            TenantModule.CreateInactive(BetaTenantId, ModuleCodes.CoreMasterData, BetaUserId, "Not subscribed").Value);
        dbContext.TenantModules.Add(
            TenantModule.CreateInactive(BetaTenantId, ModuleCodes.FinGl, BetaUserId, "Not subscribed").Value);
        dbContext.TenantModules.Add(
            TenantModule.CreateInactive(BetaTenantId, ModuleCodes.FinAr, BetaUserId, "Not subscribed").Value);
        dbContext.TenantModules.Add(
            TenantModule.CreateInactive(BetaTenantId, ModuleCodes.HrisAttendance, BetaUserId, "Not subscribed").Value);
        dbContext.TenantModules.Add(
            TenantModule.CreateInactive(BetaTenantId, ModuleCodes.HrisPayroll, BetaUserId, "Not subscribed").Value);
        dbContext.TenantModules.Add(
            TenantModule.CreateInactive(BetaTenantId, ModuleCodes.ProjectCore, BetaUserId, "Not subscribed").Value);
        dbContext.TenantModules.Add(
            TenantModule.CreateInactive(BetaTenantId, ModuleCodes.AssetCore, BetaUserId, "Not subscribed").Value);
        dbContext.TenantModules.Add(
            TenantModule.CreateInactive(BetaTenantId, ModuleCodes.ProcCore, BetaUserId, "Not subscribed").Value);
        dbContext.TenantModules.Add(
            TenantModule.CreateInactive(BetaTenantId, ModuleCodes.MfgCore, BetaUserId, "Not subscribed").Value);
        dbContext.SaveChanges();

        var betaPartner = BusinessPartner.Create(
            Guid.Parse("0192f5b4-8a00-7000-8000-000000000521"),
            BetaTenantId,
            BetaBusinessPartnerCode,
            "Beta Tenant Customer",
            nameof(PartnerType.Customer),
            null,
            0m,
            now,
            BetaUserId).Value;
        var betaItem = Item.Create(
            Guid.Parse("0192f5b4-8a00-7000-8000-000000000522"),
            BetaTenantId,
            BetaItemSku,
            "Beta Tenant Item",
            "PCS",
            null,
            0m,
            0m,
            isStockTracked: false,
            now,
            BetaUserId).Value;

        masterData.BusinessPartners.Add(betaPartner);
        masterData.Items.Add(betaItem);
        masterData.SaveChanges();

        var betaWarehouse = Warehouse.Create(
            Guid.Parse("0192f5b4-8a00-7000-8000-000000000523"),
            BetaTenantId,
            BetaWarehouseCode,
            "Beta Tenant Warehouse",
            null,
            now,
            BetaUserId).Value;

        var acmeWarehouse = Warehouse.Create(
            AcmeWarehouseId,
            TenantId,
            AcmeWarehouseCode,
            "Acme Main Warehouse",
            null,
            now,
            UserId).Value;

        inventory.Warehouses.Add(betaWarehouse);
        inventory.Warehouses.Add(acmeWarehouse);
        inventory.SaveChanges();

        var betaSalesOrder = SalesOrder.Create(
            BetaSalesOrderId,
            BetaTenantId,
            "SO-BETA-0001",
            BetaSalesOrderCustomerId,
            [new SalesOrderLineInput(BetaSalesOrderLineId, BetaSalesOrderItemId, 1m, 10m)],
            now,
            BetaUserId).Value;

        sales.SalesOrders.Add(betaSalesOrder);
        sales.SaveChanges();

        var betaInvoice = ArInvoice.Create(
            BetaArInvoiceId,
            BetaTenantId,
            BetaArInvoiceNumber,
            BetaSalesOrderId,
            BetaSalesOrderCustomerId,
            10m,
            now,
            BetaUserId).Value;

        var betaJournalEntry = JournalEntry.Create(
            BetaJournalEntryId,
            BetaTenantId,
            BetaJournalEntryNumber,
            now,
            "Beta A/R invoice posting",
            [
                new JournalEntryLineInput(Guid.NewGuid(), BetaReceivableAccountId, 10m, 0m),
                new JournalEntryLineInput(Guid.NewGuid(), BetaRevenueAccountId, 0m, 10m),
            ],
            now,
            BetaUserId).Value;

        finance.ArInvoices.Add(betaInvoice);
        finance.JournalEntries.Add(betaJournalEntry);
        finance.SaveChanges();

        var acmeEmployee = Employee.Create(
            AcmeEmployeeId,
            TenantId,
            "EMP-0001",
            "Acme Test Employee",
            "employee@acme.test",
            "Engineer",
            AcmeDepartmentId,
            new DateOnly(2026, 1, 5)).Value;

        hris.Employees.Add(acmeEmployee);
        hris.SaveChanges();

        var acmeProject = Project.Create(
            AcmeProjectId,
            TenantId,
            "PRJ-0001",
            "Acme Integration Project",
            "Project used by Phase 6 integration tests.",
            AcmeProjectCustomerId,
            new DateOnly(2026, 1, 5),
            null).Value;

        projects.Projects.Add(acmeProject);
        projects.SaveChanges();

        var acmeSupplier = Supplier.Create(
            AcmeSupplierId,
            TenantId,
            AcmeSupplierCode,
            "Acme Industrial Supplies",
            "Nadia Prasetyo",
            "orders@acme-supplies.test",
            "+62-21-555-0101",
            30,
            now,
            UserId).Value;

        var betaSupplier = Supplier.Create(
            BetaSupplierId,
            BetaTenantId,
            BetaSupplierCode,
            "Beta Tenant Vendor",
            null,
            null,
            null,
            14,
            now,
            BetaUserId).Value;

        procurement.Suppliers.Add(acmeSupplier);
        procurement.Suppliers.Add(betaSupplier);
        procurement.SaveChanges();

        var acmeBillOfMaterial = BillOfMaterial.Create(
            AcmeBomId,
            TenantId,
            AcmeBomCode,
            "Acme Widget Assembly",
            AcmeFinishedGoodItemId,
            1,
            "Bill of material used by Phase 9 integration tests.",
            [new BillOfMaterialComponentInput(AcmeBomComponentId, AcmeBomComponentItemId, 2.5m)],
            now,
            UserId).Value;

        var betaBillOfMaterial = BillOfMaterial.Create(
            BetaBomId,
            BetaTenantId,
            BetaBomCode,
            "Beta Tenant Assembly",
            BetaFinishedGoodItemId,
            1,
            null,
            [new BillOfMaterialComponentInput(BetaBomComponentId, BetaBomComponentItemId, 1m)],
            now,
            BetaUserId).Value;

        manufacturing.BillOfMaterials.Add(acmeBillOfMaterial);
        manufacturing.BillOfMaterials.Add(betaBillOfMaterial);
        manufacturing.SaveChanges();
    }
}
