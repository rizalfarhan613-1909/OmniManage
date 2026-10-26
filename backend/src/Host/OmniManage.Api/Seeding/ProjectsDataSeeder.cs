using Bogus;
using Microsoft.EntityFrameworkCore;
using OmniManage.BuildingBlocks.Application;
using OmniManage.Modules.Projects.Domain.Projects;
using OmniManage.Modules.Projects.Domain.Tasks;
using OmniManage.Modules.Projects.Domain.WorkLogs;
using OmniManage.Modules.Projects.Infrastructure.Persistence;

namespace OmniManage.Api.Seeding;

/// <summary>
/// Generates projects, their delivery tasks and the work logs recorded against
/// those tasks. Project codes are stable so the seeder can safely run again.
/// </summary>
internal static class ProjectsDataSeeder
{
    private const int TargetProjectCount = 24;
    private const int TargetTaskCount = 120;
    private const int TargetWorkLogCount = 140;

    public static async Task SeedAsync(
        ProjectsDbContext context,
        Guid tenantId,
        IReadOnlyList<Guid> customerIds,
        IReadOnlyList<Guid> employeeIds,
        IGuidGenerator guidGenerator,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(customerIds);
        ArgumentNullException.ThrowIfNull(employeeIds);
        ArgumentNullException.ThrowIfNull(guidGenerator);

        var faker = new Faker();
        var existingCodes = (await context.Projects
                .Select(project => project.Code)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false))
            .ToHashSet(StringComparer.Ordinal);
        var projects = await context.Projects
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        for (var index = 0; index < TargetProjectCount; index++)
        {
            var code = $"PRJ-{now.Year}-{index + 1:D3}";

            if (existingCodes.Contains(code))
            {
                continue;
            }

            var customerId = customerIds.Count == 0
                ? Guid.Parse("0192f5b4-8a00-7000-8000-000000009001")
                : customerIds[faker.Random.Int(0, customerIds.Count - 1)];
            var startDate = DateOnly.FromDateTime(
                now.UtcDateTime.Date.AddDays(-faker.Random.Int(30, 300)));
            DateOnly? endDate = faker.Random.Int(0, 3) == 0
                ? null
                : startDate.AddDays(faker.Random.Int(60, 240));

            var project = SeedGuard.EnsureSuccess(Project.Create(
                guidGenerator.NewGuid(),
                tenantId,
                code,
                DemoDataCatalog.ProjectNames[index % DemoDataCatalog.ProjectNames.Length],
                faker.Lorem.Sentence(12),
                customerId,
                startDate,
                endDate));

            ApplyProjectStatus(project, index);

            context.Projects.Add(project);
            projects.Add(project);
            existingCodes.Add(code);
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        if (projects.Count == 0 || employeeIds.Count == 0)
        {
            return;
        }

        await SeedTasksAsync(
                context,
                tenantId,
                projects,
                employeeIds,
                faker,
                guidGenerator,
                cancellationToken)
            .ConfigureAwait(false);
        await SeedWorkLogsAsync(
                context,
                tenantId,
                employeeIds,
                faker,
                guidGenerator,
                now,
                cancellationToken)
            .ConfigureAwait(false);
    }

    private static void ApplyProjectStatus(Project project, int index)
    {
        switch (index % 6)
        {
            case 1:
            case 2:
                SeedGuard.EnsureSuccess(project.Activate());
                break;
            case 3:
                SeedGuard.EnsureSuccess(project.Activate());
                SeedGuard.EnsureSuccess(project.PutOnHold());
                break;
            case 4:
                SeedGuard.EnsureSuccess(project.Activate());
                SeedGuard.EnsureSuccess(project.Complete());
                break;
            case 5:
                SeedGuard.EnsureSuccess(project.Cancel());
                break;
            case 0:
            default:
                break;
        }
    }

    private static async Task SeedTasksAsync(
        ProjectsDbContext context,
        Guid tenantId,
        IReadOnlyList<Project> projects,
        IReadOnlyList<Guid> employeeIds,
        Faker faker,
        IGuidGenerator guidGenerator,
        CancellationToken cancellationToken)
    {
        var existingTaskCount = await context.Tasks
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);

        if (existingTaskCount >= TargetTaskCount)
        {
            return;
        }

        var tasksToCreate = TargetTaskCount - existingTaskCount;

        for (var index = 0; index < tasksToCreate; index++)
        {
            var project = projects[faker.Random.Int(0, projects.Count - 1)];
            var assigneeId = employeeIds[faker.Random.Int(0, employeeIds.Count - 1)];
            var estimatedHours = decimal.Round(faker.Random.Decimal(4m, 40m), 2);
            var progressHours = decimal.Round(
                estimatedHours * faker.Random.Decimal(0.3m, 0.9m),
                2);

            if (progressHours <= 0m)
            {
                progressHours = 0.5m;
            }

            var task = SeedGuard.EnsureSuccess(ProjectTask.Create(
                guidGenerator.NewGuid(),
                tenantId,
                project.Id,
                DemoDataCatalog.TaskTitles[index % DemoDataCatalog.TaskTitles.Length],
                assigneeId,
                estimatedHours));

            var statusSelector = index % 10;

            if (statusSelector is >= 4 and <= 6)
            {
                SeedGuard.EnsureSuccess(task.Start());
                SeedGuard.EnsureSuccess(task.RecordProgress(progressHours));
                SeedGuard.EnsureSuccess(task.Complete());
            }
            else if (statusSelector == 7)
            {
                SeedGuard.EnsureSuccess(task.Start());
                SeedGuard.EnsureSuccess(task.Block());
            }
            else if (statusSelector < 4)
            {
                SeedGuard.EnsureSuccess(task.Start());
                SeedGuard.EnsureSuccess(task.RecordProgress(progressHours));
            }

            context.Tasks.Add(task);
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task SeedWorkLogsAsync(
        ProjectsDbContext context,
        Guid tenantId,
        IReadOnlyList<Guid> employeeIds,
        Faker faker,
        IGuidGenerator guidGenerator,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var existingWorkLogCount = await context.WorkLogs
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);

        if (existingWorkLogCount >= TargetWorkLogCount)
        {
            return;
        }

        var taskIds = await context.Tasks
            .Select(task => task.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (taskIds.Count == 0 || employeeIds.Count == 0)
        {
            return;
        }

        var workLogsToCreate = TargetWorkLogCount - existingWorkLogCount;

        for (var index = 0; index < workLogsToCreate; index++)
        {
            var loggedDate = DateOnly.FromDateTime(
                now.UtcDateTime.Date.AddDays(-faker.Random.Int(0, 60)));
            var workLog = SeedGuard.EnsureSuccess(WorkLog.Create(
                guidGenerator.NewGuid(),
                tenantId,
                taskIds[faker.Random.Int(0, taskIds.Count - 1)],
                employeeIds[faker.Random.Int(0, employeeIds.Count - 1)],
                loggedDate,
                decimal.Round(faker.Random.Decimal(0.5m, 8m), 2),
                faker.PickRandom(DemoDataCatalog.WorkLogDescriptions)));

            context.WorkLogs.Add(workLog);
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
