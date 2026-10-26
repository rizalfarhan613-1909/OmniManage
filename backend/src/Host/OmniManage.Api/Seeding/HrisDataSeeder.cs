using System.Globalization;
using Bogus;
using Microsoft.EntityFrameworkCore;
using OmniManage.BuildingBlocks.Application;
using OmniManage.Modules.Hris.Domain.Attendance;
using OmniManage.Modules.Hris.Domain.Employees;
using OmniManage.Modules.Hris.Domain.Payroll;
using OmniManage.Modules.Hris.Infrastructure.Persistence;

namespace OmniManage.Api.Seeding;

/// <summary>
/// Generates a realistic HRIS fixture: employees, smart attendance logs and
/// payroll runs. Employees are matched by their stable employee code, while
/// attendance logs and payroll runs are only created when the target volume
/// has not been reached yet.
/// </summary>
internal static class HrisDataSeeder
{
    private const int TargetEmployeeCount = 120;
    private const int TargetAttendanceCount = 140;
    private const int TargetPayrollCount = 14;

    private const decimal OfficeLatitude = -6.2088m;
    private const decimal OfficeLongitude = 106.8456m;

    public static async Task<IReadOnlyList<Guid>> SeedAsync(
        HrisDbContext context,
        Guid tenantId,
        IGuidGenerator guidGenerator,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(guidGenerator);

        var faker = new Faker();
        var existingCodes = await context.Employees
            .Select(employee => employee.EmployeeCode)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var knownCodes = existingCodes.ToHashSet(StringComparer.Ordinal);
        var knownEmails = (await context.Employees
                .Select(employee => employee.Email)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        for (var index = 0; index < TargetEmployeeCount; index++)
        {
            var employeeCode = $"EMP-{index + 1:D4}";

            if (knownCodes.Contains(employeeCode))
            {
                continue;
            }

            var department = DemoDataCatalog.Departments[index % DemoDataCatalog.Departments.Length];
            var firstName = DemoDataCatalog.FirstNames[index % DemoDataCatalog.FirstNames.Length];
            var lastName = DemoDataCatalog.LastNames[
                (index / DemoDataCatalog.FirstNames.Length + index * 7) % DemoDataCatalog.LastNames.Length];
            var fullName = $"{firstName} {lastName}";
            var email = BuildUniqueEmail(firstName, lastName, index, knownEmails);
            var jobTitle = faker.PickRandom(department.JobTitles);
            var joinedDate = DateOnly.FromDateTime(
                now.UtcDateTime.Date.AddDays(-faker.Random.Int(30, 2_000)));

            var employee = SeedGuard.EnsureSuccess(Employee.Create(
                guidGenerator.NewGuid(),
                tenantId,
                employeeCode,
                fullName,
                email,
                jobTitle,
                department.Id,
                joinedDate));

            context.Employees.Add(employee);
            knownCodes.Add(employeeCode);
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var employees = await context.Employees
            .Select(employee => new EmployeeReference(employee.Id, employee.EmployeeCode))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (employees.Count == 0)
        {
            return [];
        }

        await SeedAttendanceAsync(context, tenantId, employees, faker, guidGenerator, now, cancellationToken)
            .ConfigureAwait(false);
        await SeedPayrollRunsAsync(context, tenantId, faker, guidGenerator, now, cancellationToken)
            .ConfigureAwait(false);

        return employees.Select(employee => employee.Id).ToArray();
    }

    private static async Task SeedAttendanceAsync(
        HrisDbContext context,
        Guid tenantId,
        IReadOnlyList<EmployeeReference> employees,
        Faker faker,
        IGuidGenerator guidGenerator,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var existingCount = await context.AttendanceLogs
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);

        if (existingCount >= TargetAttendanceCount)
        {
            return;
        }

        var employeeIds = employees.Select(employee => employee.Id).ToArray();
        var employeeCodes = employees.ToDictionary(
            employee => employee.Id,
            employee => employee.EmployeeCode);

        for (var index = existingCount; index < TargetAttendanceCount; index++)
        {
            var employeeId = employeeIds[faker.Random.Int(0, employeeIds.Length - 1)];
            var daysAgo = faker.Random.Int(0, 45);
            var clockIn = new DateTimeOffset(now.UtcDateTime.Date, TimeSpan.Zero)
                .AddDays(-daysAgo)
                .AddHours(7)
                .AddMinutes(faker.Random.Int(15, 75));
            var latitude = decimal.Round(
                OfficeLatitude + (decimal)faker.Random.Double(-0.002, 0.002),
                6,
                MidpointRounding.AwayFromZero);
            var longitude = decimal.Round(
                OfficeLongitude + (decimal)faker.Random.Double(-0.002, 0.002),
                6,
                MidpointRounding.AwayFromZero);
            var photoUrl =
                $"https://cdn.omnimanage.local/attendance/{employeeCodes[employeeId]}/" +
                $"{clockIn.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}.jpg";

            var log = SeedGuard.EnsureSuccess(SmartAttendanceLog.Create(
                guidGenerator.NewGuid(),
                tenantId,
                employeeId,
                clockIn,
                latitude,
                longitude,
                photoUrl));

            if (index % 5 != 0)
            {
                SeedGuard.EnsureSuccess(log.ClockOut(
                    clockIn.AddHours(8).AddMinutes(faker.Random.Int(0, 90))));
            }

            context.AttendanceLogs.Add(log);
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task SeedPayrollRunsAsync(
        HrisDbContext context,
        Guid tenantId,
        Faker faker,
        IGuidGenerator guidGenerator,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var existingPeriods = (await context.PayrollRuns
                .Select(payrollRun => payrollRun.PeriodCode)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var currentMonth = new DateTimeOffset(
            now.Year,
            now.Month,
            1,
            0,
            0,
            0,
            TimeSpan.Zero);

        for (var offset = TargetPayrollCount - 1; offset >= 0; offset--)
        {
            var periodStart = currentMonth.AddMonths(-offset);
            var periodCode = periodStart.ToString("yyyy-MM", CultureInfo.InvariantCulture);

            if (existingPeriods.Contains(periodCode))
            {
                continue;
            }

            var payrollRun = SeedGuard.EnsureSuccess(PayrollRun.Create(
                guidGenerator.NewGuid(),
                tenantId,
                periodCode,
                decimal.Round(faker.Random.Decimal(1_450_000_000m, 2_350_000_000m), 2),
                decimal.Round(faker.Random.Decimal(120_000_000m, 320_000_000m), 2),
                decimal.Round(faker.Random.Decimal(90_000_000m, 240_000_000m), 2)));

            // Keep the two most recent periods as draft (pending) runs so the
            // UI can exercise both the pending and finalized states.
            if (offset >= 2)
            {
                SeedGuard.EnsureSuccess(payrollRun.Finalize(periodStart.AddMonths(1).AddDays(-1)));
            }

            context.PayrollRuns.Add(payrollRun);
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static string BuildUniqueEmail(
        string firstName,
        string lastName,
        int index,
        ISet<string> knownEmails)
    {
        var email = $"{firstName}.{lastName}@acme.co.id".ToLowerInvariant();

        if (knownEmails.Add(email))
        {
            return email;
        }

        email = $"{firstName}.{lastName}{index + 1:D3}@acme.co.id".ToLowerInvariant();
        knownEmails.Add(email);

        return email;
    }

    private sealed record EmployeeReference(Guid Id, string EmployeeCode);
}
