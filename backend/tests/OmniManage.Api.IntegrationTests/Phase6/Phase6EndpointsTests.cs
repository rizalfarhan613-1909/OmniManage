using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OmniManage.Api.IntegrationTests;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.Modules.AssetMaintenance.Domain.Assets;
using OmniManage.Modules.AssetMaintenance.Domain.Maintenance;
using OmniManage.Modules.AssetMaintenance.Infrastructure.Persistence;
using OmniManage.Modules.Finance.Infrastructure.Persistence;
using OmniManage.Modules.Hris.Domain.Attendance;
using OmniManage.Modules.Hris.Domain.Payroll;
using OmniManage.Modules.Hris.Infrastructure.Persistence;
using OmniManage.Modules.Platform.Application.Auth;
using OmniManage.Modules.Projects.Domain.Tasks;
using OmniManage.Modules.Projects.Infrastructure.Persistence;

namespace OmniManage.Api.IntegrationTests.Phase6;

public enum Phase6Endpoint
{
    AttendanceClockIn,
    PayrollFinalize,
    ProjectTaskCreate,
    WorkLogCreate,
    AssetCreate,
    MaintenanceOrderCreate,
}

public sealed class Phase6EndpointsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private const string ClockInPath = "/api/v1/hris/attendance/clock-in";
    private const string PayrollRunsPath = "/api/v1/hris/payroll/runs";
    private const string TasksPath = "/api/v1/projects/tasks";
    private const string AssetsPath = "/api/v1/assets";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Theory]
    [InlineData(Phase6Endpoint.AttendanceClockIn, true)]
    [InlineData(Phase6Endpoint.AttendanceClockIn, false)]
    [InlineData(Phase6Endpoint.PayrollFinalize, true)]
    [InlineData(Phase6Endpoint.PayrollFinalize, false)]
    [InlineData(Phase6Endpoint.ProjectTaskCreate, true)]
    [InlineData(Phase6Endpoint.ProjectTaskCreate, false)]
    [InlineData(Phase6Endpoint.WorkLogCreate, true)]
    [InlineData(Phase6Endpoint.WorkLogCreate, false)]
    [InlineData(Phase6Endpoint.AssetCreate, true)]
    [InlineData(Phase6Endpoint.AssetCreate, false)]
    [InlineData(Phase6Endpoint.MaintenanceOrderCreate, true)]
    [InlineData(Phase6Endpoint.MaintenanceOrderCreate, false)]
    public async Task LicenseMatrix_EnforcesModuleState(Phase6Endpoint endpoint, bool moduleActive)
    {
        if (moduleActive)
        {
            using var activeClient = await CreateAuthorizedClientAsync();

            var response = await SendActiveRequestAsync(activeClient, endpoint);

            response.StatusCode.Should().Be(ExpectedSuccessStatus(endpoint));
            return;
        }

        using var inactiveClient = await CreateAuthorizedClientAsync(
            ApiFactory.BetaEmail,
            ApiFactory.BetaTenantCode);

        var deniedResponse = await SendDeniedRequestAsync(inactiveClient, endpoint);

        deniedResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var payload = await deniedResponse.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Success.Should().BeFalse();
        payload.Error.Should().NotBeNull();
        payload.Error!.Code.Should().Be("PLATFORM.MODULE_NOT_LICENSED");
        payload.Error.Details.Should().ContainSingle()
            .Which.Field.Should().Be("moduleCode");
        payload.Error.Details.Should().ContainSingle()
            .Which.Message.Should().Be(ExpectedModuleCode(endpoint));
    }

    [Theory]
    [InlineData("/api/v1/hris/attendance")]
    [InlineData("/api/v1/projects/tasks")]
    [InlineData("/api/v1/assets")]
    public async Task ListEndpoints_ReturnPagedEnvelopeAndHonorPageQuery(string path)
    {
        using var client = await CreateAuthorizedClientAsync();

        var response = await client.GetAsync(new Uri($"{path}?page=2&pageSize=5", UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = document.RootElement;

        // The BFF treats any body without a `success` flag as an unreadable
        // upstream response, so list endpoints must return the standard envelope.
        root.GetProperty("success").GetBoolean().Should().BeTrue();

        var data = root.GetProperty("data");
        data.GetProperty("page").GetInt32().Should().Be(2);
        data.GetProperty("pageSize").GetInt32().Should().Be(5);
        data.GetProperty("items").ValueKind.Should().Be(JsonValueKind.Array);

        var pagination = root.GetProperty("meta").GetProperty("pagination");
        pagination.GetProperty("page").GetInt32().Should().Be(2);
        pagination.GetProperty("pageSize").GetInt32().Should().Be(5);
    }

    [Fact]
    public async Task ClockIn_WithActiveLicense_PersistsAttendanceAndOutbox()
    {
        using var client = await CreateAuthorizedClientAsync();

        var response = await client.PostAsJsonAsync(
            ClockInPath,
            ClockInBody(ApiFactory.AcmeEmployeeId),
            JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Success.Should().BeTrue();

        var attendanceLogId = payload.Data;
        attendanceLogId.Should().NotBe(Guid.Empty);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Be($"/api/v1/hris/attendance/{attendanceLogId:D}");

        using var scope = factory.Services.CreateScope();
        var hris = scope.ServiceProvider.GetRequiredService<HrisDbContext>();

        var attendanceLog = await hris.AttendanceLogs
            .IgnoreQueryFilters()
            .AsNoTracking()
            .SingleOrDefaultAsync(log => log.Id == attendanceLogId);

        attendanceLog.Should().NotBeNull();
        attendanceLog!.TenantId.Should().Be(ApiFactory.TenantId);
        attendanceLog.EmployeeId.Should().Be(ApiFactory.AcmeEmployeeId);
        attendanceLog.Status.Should().Be(AttendanceStatus.ClockedIn);

        var outboxMessage = await hris.OutboxMessages
            .IgnoreQueryFilters()
            .AsNoTracking()
            .SingleOrDefaultAsync(message =>
                message.Type.Contains(
                    "SmartAttendanceClockedInIntegrationEvent",
                    StringComparison.Ordinal)
                && message.Payload.Contains(
                    attendanceLogId.ToString(),
                    StringComparison.OrdinalIgnoreCase));

        outboxMessage.Should().NotBeNull();
        outboxMessage!.TenantId.Should().Be(ApiFactory.TenantId);
        outboxMessage.ProcessedOnUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task FinalizePayrollRun_WithActiveLicense_FinalizesRunAndPostsJournal()
    {
        using var client = await CreateAuthorizedClientAsync();
        var payrollRunId = await SeedPayrollRunAsync();

        var response = await client.PostAsync(
            new Uri($"{PayrollRunsPath}/{payrollRunId:D}/finalize", UriKind.Relative),
            content: null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = factory.Services.CreateScope();
        var hris = scope.ServiceProvider.GetRequiredService<HrisDbContext>();

        var payrollRun = await hris.PayrollRuns
            .IgnoreQueryFilters()
            .AsNoTracking()
            .SingleOrDefaultAsync(run => run.Id == payrollRunId);

        payrollRun.Should().NotBeNull();
        payrollRun!.Status.Should().Be(PayrollStatus.Finalized);
        payrollRun.ProcessedAtUtc.Should().NotBeNull();

        var outboxMessage = await hris.OutboxMessages
            .IgnoreQueryFilters()
            .AsNoTracking()
            .SingleOrDefaultAsync(message =>
                message.Type.Contains("PayrollFinalizedIntegrationEvent", StringComparison.Ordinal)
                && message.Payload.Contains(
                    payrollRunId.ToString(),
                    StringComparison.OrdinalIgnoreCase));

        outboxMessage.Should().NotBeNull();
        outboxMessage!.ProcessedOnUtc.Should().NotBeNull();

        var finance = scope.ServiceProvider.GetRequiredService<FinanceDbContext>();
        var entryNumber = $"PAYROLL-{payrollRunId:D}".ToUpperInvariant();

        var journalEntry = await finance.JournalEntries
            .IgnoreQueryFilters()
            .AsNoTracking()
            .SingleOrDefaultAsync(entry => entry.EntryNumber == entryNumber);

        journalEntry.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateProjectTask_WithActiveLicense_PersistsTask()
    {
        using var client = await CreateAuthorizedClientAsync();
        var title = UniqueTitle("TASK");

        var response = await CreateTaskAsync(client, title);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Success.Should().BeTrue();

        var taskId = payload.Data;
        taskId.Should().NotBe(Guid.Empty);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Be($"{TasksPath}/{taskId:D}");

        using var scope = factory.Services.CreateScope();
        var projects = scope.ServiceProvider.GetRequiredService<ProjectsDbContext>();

        var task = await projects.Tasks
            .IgnoreQueryFilters()
            .AsNoTracking()
            .SingleOrDefaultAsync(projectTask => projectTask.Id == taskId);

        task.Should().NotBeNull();
        task!.TenantId.Should().Be(ApiFactory.TenantId);
        task.ProjectId.Should().Be(ApiFactory.AcmeProjectId);
        task.Title.Should().Be(title);
        task.Status.Should().Be(ProjectTaskStatus.Todo);
        task.EstimatedHours.Should().Be(8m);
        task.CompletedHours.Should().Be(0m);
    }

    [Fact]
    public async Task RecordWorkLog_WithActiveLicense_UpdatesTaskAndPublishesCompletion()
    {
        using var client = await CreateAuthorizedClientAsync();
        var taskId = await CreateTaskOrThrowAsync(client, UniqueTitle("LOG"));

        var response = await RecordWorkLogAsync(client, taskId, completeTask: true);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Success.Should().BeTrue();

        var workLogId = payload.Data;
        workLogId.Should().NotBe(Guid.Empty);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString()
            .Should().Be($"{TasksPath}/{taskId:D}/work-logs/{workLogId:D}");

        using var scope = factory.Services.CreateScope();
        var projects = scope.ServiceProvider.GetRequiredService<ProjectsDbContext>();

        var task = await projects.Tasks
            .IgnoreQueryFilters()
            .AsNoTracking()
            .SingleOrDefaultAsync(projectTask => projectTask.Id == taskId);

        task.Should().NotBeNull();
        task!.Status.Should().Be(ProjectTaskStatus.Done);
        task.CompletedHours.Should().Be(2.5m);

        var workLog = await projects.WorkLogs
            .IgnoreQueryFilters()
            .AsNoTracking()
            .SingleOrDefaultAsync(log => log.Id == workLogId);

        workLog.Should().NotBeNull();
        workLog!.TenantId.Should().Be(ApiFactory.TenantId);
        workLog.ProjectTaskId.Should().Be(taskId);
        workLog.EmployeeId.Should().Be(ApiFactory.AcmeEmployeeId);
        workLog.HoursSpent.Should().Be(2.5m);

        var outboxMessage = await projects.OutboxMessages
            .IgnoreQueryFilters()
            .AsNoTracking()
            .SingleOrDefaultAsync(message =>
                message.Type.Contains("TaskCompletedIntegrationEvent", StringComparison.Ordinal)
                && message.Payload.Contains(
                    taskId.ToString(),
                    StringComparison.OrdinalIgnoreCase));

        outboxMessage.Should().NotBeNull();
        outboxMessage!.ProcessedOnUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateAsset_WithActiveLicense_PersistsAsset()
    {
        using var client = await CreateAuthorizedClientAsync();
        var assetCode = UniqueAssetCode("AST");

        var response = await CreateAssetAsync(client, assetCode);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Success.Should().BeTrue();

        var assetId = payload.Data;
        assetId.Should().NotBe(Guid.Empty);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Be($"{AssetsPath}/{assetId:D}");

        using var scope = factory.Services.CreateScope();
        var assets = scope.ServiceProvider.GetRequiredService<AssetMaintenanceDbContext>();

        var asset = await assets.Assets
            .IgnoreQueryFilters()
            .AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == assetId);

        asset.Should().NotBeNull();
        asset!.TenantId.Should().Be(ApiFactory.TenantId);
        asset.AssetCode.Should().Be(assetCode);
        asset.Status.Should().Be(AssetStatus.Active);
        asset.PurchaseValue.Should().Be(1250.5m);
    }

    [Fact]
    public async Task RequestMaintenance_WithActiveLicense_PersistsOrderAndOutbox()
    {
        using var client = await CreateAuthorizedClientAsync();
        var assetId = await CreateAssetOrThrowAsync(client, UniqueAssetCode("MNT"));
        var requestNumber = UniqueRequestNumber("MR");

        var response = await RequestMaintenanceAsync(client, assetId, requestNumber);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Success.Should().BeTrue();

        var maintenanceOrderId = payload.Data;
        maintenanceOrderId.Should().NotBe(Guid.Empty);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString()
            .Should().Be($"{AssetsPath}/{assetId:D}/maintenance-orders/{maintenanceOrderId:D}");

        using var scope = factory.Services.CreateScope();
        var assetMaintenance = scope.ServiceProvider.GetRequiredService<AssetMaintenanceDbContext>();

        var maintenanceOrder = await assetMaintenance.MaintenanceOrders
            .IgnoreQueryFilters()
            .AsNoTracking()
            .SingleOrDefaultAsync(order => order.Id == maintenanceOrderId);

        maintenanceOrder.Should().NotBeNull();
        maintenanceOrder!.TenantId.Should().Be(ApiFactory.TenantId);
        maintenanceOrder.AssetId.Should().Be(assetId);
        maintenanceOrder.RequestNumber.Should().Be(requestNumber);
        maintenanceOrder.Status.Should().Be(MaintenanceStatus.Scheduled);

        var outboxMessage = await assetMaintenance.OutboxMessages
            .IgnoreQueryFilters()
            .AsNoTracking()
            .SingleOrDefaultAsync(message =>
                message.Type.Contains(
                    "AssetMaintenanceRequestedIntegrationEvent",
                    StringComparison.Ordinal)
                && message.Payload.Contains(
                    assetId.ToString(),
                    StringComparison.OrdinalIgnoreCase));

        outboxMessage.Should().NotBeNull();
        outboxMessage!.ProcessedOnUtc.Should().NotBeNull();
    }

    private static HttpStatusCode ExpectedSuccessStatus(Phase6Endpoint endpoint) =>
        endpoint == Phase6Endpoint.PayrollFinalize
            ? HttpStatusCode.OK
            : HttpStatusCode.Created;

    private static string ExpectedModuleCode(Phase6Endpoint endpoint) =>
        endpoint switch
        {
            Phase6Endpoint.AttendanceClockIn => ModuleCodes.HrisAttendance,
            Phase6Endpoint.PayrollFinalize => ModuleCodes.HrisPayroll,
            Phase6Endpoint.ProjectTaskCreate or Phase6Endpoint.WorkLogCreate => ModuleCodes.ProjectCore,
            Phase6Endpoint.AssetCreate or Phase6Endpoint.MaintenanceOrderCreate => ModuleCodes.AssetCore,
            _ => throw new ArgumentOutOfRangeException(nameof(endpoint), endpoint, "Unknown Phase 6 endpoint."),
        };

    private async Task<HttpResponseMessage> SendActiveRequestAsync(
        HttpClient client,
        Phase6Endpoint endpoint)
    {
        switch (endpoint)
        {
            case Phase6Endpoint.AttendanceClockIn:
                return await client.PostAsJsonAsync(
                    ClockInPath,
                    ClockInBody(ApiFactory.AcmeEmployeeId),
                    JsonOptions);
            case Phase6Endpoint.PayrollFinalize:
            {
                var payrollRunId = await SeedPayrollRunAsync();

                return await client.PostAsync(
                    new Uri($"{PayrollRunsPath}/{payrollRunId:D}/finalize", UriKind.Relative),
                    content: null);
            }

            case Phase6Endpoint.ProjectTaskCreate:
                return await CreateTaskAsync(client, UniqueTitle("TASK"));
            case Phase6Endpoint.WorkLogCreate:
            {
                var taskId = await CreateTaskOrThrowAsync(client, UniqueTitle("LOG"));

                return await RecordWorkLogAsync(client, taskId, completeTask: false);
            }

            case Phase6Endpoint.AssetCreate:
                return await CreateAssetAsync(client, UniqueAssetCode("AST"));
            case Phase6Endpoint.MaintenanceOrderCreate:
            {
                var assetId = await CreateAssetOrThrowAsync(client, UniqueAssetCode("MNT"));

                return await RequestMaintenanceAsync(client, assetId, UniqueRequestNumber("MR"));
            }

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(endpoint),
                    endpoint,
                    "Unknown Phase 6 endpoint.");
        }
    }

    private static Task<HttpResponseMessage> SendDeniedRequestAsync(
        HttpClient client,
        Phase6Endpoint endpoint)
    {
        var placeholderId = Guid.NewGuid();

        return endpoint switch
        {
            Phase6Endpoint.AttendanceClockIn => client.PostAsJsonAsync(
                ClockInPath,
                ClockInBody(placeholderId),
                JsonOptions),
            Phase6Endpoint.PayrollFinalize => client.PostAsync(
                new Uri($"{PayrollRunsPath}/{placeholderId:D}/finalize", UriKind.Relative),
                content: null),
            Phase6Endpoint.ProjectTaskCreate => client.PostAsJsonAsync(
                TasksPath,
                TaskBody(placeholderId, "Denied task", placeholderId),
                JsonOptions),
            Phase6Endpoint.WorkLogCreate => client.PostAsJsonAsync(
                new Uri($"{TasksPath}/{placeholderId:D}/work-logs", UriKind.Relative),
                WorkLogBody(placeholderId, completeTask: false),
                JsonOptions),
            Phase6Endpoint.AssetCreate => client.PostAsJsonAsync(
                AssetsPath,
                AssetBody(UniqueAssetCode("DENIED")),
                JsonOptions),
            Phase6Endpoint.MaintenanceOrderCreate => client.PostAsJsonAsync(
                new Uri($"{AssetsPath}/{placeholderId:D}/maintenance-orders", UriKind.Relative),
                MaintenanceBody(UniqueRequestNumber("DENIED")),
                JsonOptions),
            _ => throw new ArgumentOutOfRangeException(
                nameof(endpoint),
                endpoint,
                "Unknown Phase 6 endpoint."),
        };
    }

    private async Task<HttpClient> CreateAuthorizedClientAsync(
        string email = ApiFactory.Email,
        string tenantCode = ApiFactory.TenantCode)
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new
            {
                tenantCode,
                email,
                password = ApiFactory.Password,
            },
            JsonOptions);

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Data.Should().NotBeNull();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", payload.Data!.AccessToken);

        return client;
    }

    private async Task<Guid> SeedPayrollRunAsync()
    {
        using var scope = factory.Services.CreateScope();
        var hris = scope.ServiceProvider.GetRequiredService<HrisDbContext>();

        var created = PayrollRun.Create(
            Guid.NewGuid(),
            ApiFactory.TenantId,
            UniquePeriodCode(),
            1000m,
            100m,
            50m);

        created.IsSuccess.Should().BeTrue();

        hris.PayrollRuns.Add(created.Value);
        await hris.SaveChangesAsync();

        return created.Value.Id;
    }

    private static Task<HttpResponseMessage> CreateTaskAsync(HttpClient client, string title) =>
        client.PostAsJsonAsync(
            TasksPath,
            TaskBody(ApiFactory.AcmeProjectId, title, ApiFactory.AcmeEmployeeId),
            JsonOptions);

    private static async Task<Guid> CreateTaskOrThrowAsync(HttpClient client, string title)
    {
        var response = await CreateTaskAsync(client, title);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Success.Should().BeTrue();

        return payload.Data;
    }

    private static Task<HttpResponseMessage> RecordWorkLogAsync(
        HttpClient client,
        Guid taskId,
        bool completeTask) =>
        client.PostAsJsonAsync(
            new Uri($"{TasksPath}/{taskId:D}/work-logs", UriKind.Relative),
            WorkLogBody(ApiFactory.AcmeEmployeeId, completeTask),
            JsonOptions);

    private static Task<HttpResponseMessage> CreateAssetAsync(HttpClient client, string assetCode) =>
        client.PostAsJsonAsync(AssetsPath, AssetBody(assetCode), JsonOptions);

    private static async Task<Guid> CreateAssetOrThrowAsync(HttpClient client, string assetCode)
    {
        var response = await CreateAssetAsync(client, assetCode);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Success.Should().BeTrue();

        return payload.Data;
    }

    private static Task<HttpResponseMessage> RequestMaintenanceAsync(
        HttpClient client,
        Guid assetId,
        string requestNumber) =>
        client.PostAsJsonAsync(
            new Uri($"{AssetsPath}/{assetId:D}/maintenance-orders", UriKind.Relative),
            MaintenanceBody(requestNumber),
            JsonOptions);

    private static object ClockInBody(Guid employeeId) => new
    {
        employeeId,
        latitude = -6.200000m,
        longitude = 106.816666m,
        photoUrl = "https://cdn.acme.test/attendance/selfie.jpg",
        attendanceMode = "GPS_PHOTO",
        wifiSsid = "ACME-OFFICE",
    };

    private static object TaskBody(Guid projectId, string title, Guid assigneeEmployeeId) => new
    {
        projectId,
        title,
        assigneeEmployeeId,
        estimatedHours = 8m,
    };

    private static object WorkLogBody(Guid employeeId, bool completeTask) => new
    {
        employeeId,
        loggedDate = new DateOnly(2026, 10, 21),
        hoursSpent = 2.5m,
        description = "Phase 6 integration test work log.",
        completeTask,
    };

    private static object AssetBody(string assetCode) => new
    {
        assetCode,
        name = "Phase 6 Test Laptop",
        category = "IT",
        serialNumber = $"SN-{assetCode}",
        qrCodeUrl = "https://cdn.acme.test/assets/qr.png",
        purchaseValue = 1250.5m,
        acquisitionDate = new DateOnly(2026, 10, 1),
    };

    private static object MaintenanceBody(string requestNumber) => new
    {
        requestNumber,
        scheduledDate = new DateOnly(2026, 10, 25),
        description = "Phase 6 integration test maintenance request.",
        maintenanceType = "Preventive",
        priority = "High",
    };

    private static string UniqueTitle(string prefix) =>
        $"{prefix}-{Guid.NewGuid().ToString("N")[..12].ToUpperInvariant()}";

    private static string UniqueAssetCode(string prefix) =>
        $"{prefix}-{Guid.NewGuid().ToString("N")[..12].ToUpperInvariant()}";

    private static string UniqueRequestNumber(string prefix) =>
        $"{prefix}-{Guid.NewGuid().ToString("N")[..12].ToUpperInvariant()}";

    private static string UniquePeriodCode() =>
        $"PAY-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
}
