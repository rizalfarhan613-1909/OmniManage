using Bogus;
using Microsoft.EntityFrameworkCore;
using OmniManage.BuildingBlocks.Application;
using OmniManage.Modules.AssetMaintenance.Domain.Assets;
using OmniManage.Modules.AssetMaintenance.Domain.Maintenance;
using OmniManage.Modules.AssetMaintenance.Infrastructure.Persistence;

namespace OmniManage.Api.Seeding;

/// <summary>
/// Generates the asset register and the maintenance work orders raised against
/// it. Asset codes and maintenance request numbers are stable for idempotency.
/// </summary>
internal static class AssetMaintenanceDataSeeder
{
    private const int TargetAssetCount = 120;
    private const int TargetMaintenanceCount = 60;

    public static async Task SeedAsync(
        AssetMaintenanceDbContext context,
        Guid tenantId,
        IGuidGenerator guidGenerator,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(guidGenerator);

        var faker = new Faker();
        var existingCodes = (await context.Assets
                .Select(asset => asset.AssetCode)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false))
            .ToHashSet(StringComparer.Ordinal);
        var assets = await context.Assets
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        for (var index = 0; index < TargetAssetCount; index++)
        {
            var template = DemoDataCatalog.AssetTemplates[
                index % DemoDataCatalog.AssetTemplates.Length];
            var assetCode = $"AST-{template.CodePrefix}-{index + 1:D4}";

            if (existingCodes.Contains(assetCode))
            {
                continue;
            }

            var asset = SeedGuard.EnsureSuccess(Asset.Create(
                guidGenerator.NewGuid(),
                tenantId,
                assetCode,
                template.Name,
                template.Category,
                $"SN-{faker.Random.AlphaNumeric(10).ToUpperInvariant()}",
                $"https://assets.omnimanage.local/qr/{assetCode}.png",
                decimal.Round(faker.Random.Decimal(template.MinValue, template.MaxValue), 2),
                DateOnly.FromDateTime(
                    now.UtcDateTime.Date.AddDays(-faker.Random.Int(30, 1_800)))));

            if (index % 13 == 12)
            {
                SeedGuard.EnsureSuccess(asset.SendToMaintenance());
            }
            else if (index % 19 == 18)
            {
                SeedGuard.EnsureSuccess(asset.Retire());
            }

            context.Assets.Add(asset);
            assets.Add(asset);
            existingCodes.Add(assetCode);
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        if (assets.Count == 0)
        {
            return;
        }

        await SeedMaintenanceOrdersAsync(
                context,
                tenantId,
                assets,
                faker,
                guidGenerator,
                now,
                cancellationToken)
            .ConfigureAwait(false);
    }

    private static async Task SeedMaintenanceOrdersAsync(
        AssetMaintenanceDbContext context,
        Guid tenantId,
        IReadOnlyList<Asset> assets,
        Faker faker,
        IGuidGenerator guidGenerator,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var existingRequestNumbers = (await context.MaintenanceOrders
                .Select(order => order.RequestNumber)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false))
            .ToHashSet(StringComparer.Ordinal);

        if (existingRequestNumbers.Count >= TargetMaintenanceCount)
        {
            return;
        }

        for (var index = 0; index < TargetMaintenanceCount; index++)
        {
            var requestNumber = $"MRO-{now.Year}-{index + 1:D4}";

            if (existingRequestNumbers.Contains(requestNumber))
            {
                continue;
            }

            var asset = assets[faker.Random.Int(0, assets.Count - 1)];
            var statusSelector = index % 4;
            var scheduledDate = DateOnly.FromDateTime(
                now.UtcDateTime.Date.AddDays(
                    statusSelector == 2
                        ? faker.Random.Int(-120, -10)
                        : faker.Random.Int(-60, 30)));

            var order = SeedGuard.EnsureSuccess(MaintenanceOrder.Create(
                guidGenerator.NewGuid(),
                tenantId,
                asset.Id,
                requestNumber,
                scheduledDate,
                faker.PickRandom(DemoDataCatalog.MaintenanceDescriptions)));

            switch (statusSelector)
            {
                case 1:
                    SeedGuard.EnsureSuccess(order.Start());
                    break;
                case 2:
                    SeedGuard.EnsureSuccess(order.Start());
                    SeedGuard.EnsureSuccess(order.Complete(
                        scheduledDate.AddDays(faker.Random.Int(0, 7)),
                        decimal.Round(faker.Random.Decimal(500_000m, 15_000_000m), 2)));
                    break;
                case 3:
                    SeedGuard.EnsureSuccess(order.Cancel());
                    break;
                case 0:
                default:
                    break;
            }

            context.MaintenanceOrders.Add(order);
            existingRequestNumbers.Add(requestNumber);
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
