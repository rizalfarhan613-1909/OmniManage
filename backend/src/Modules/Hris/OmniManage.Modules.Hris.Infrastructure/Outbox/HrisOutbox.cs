using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.BuildingBlocks.Infrastructure.Persistence.Outbox;
using OmniManage.Modules.Hris.Application.IntegrationEvents;
using OmniManage.Modules.Hris.Infrastructure.Persistence;

namespace OmniManage.Modules.Hris.Infrastructure.Outbox;

public sealed class HrisOutbox(HrisDbContext dbContext, TimeProvider clock) : IHrisOutbox
{
    private const int ErrorMaxLength = 4000;

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public Task EnqueueAsync(
        IIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);

        var eventType = integrationEvent.GetType();
        var message = new OutboxMessage
        {
            Id = integrationEvent.EventId,
            TenantId = integrationEvent.TenantId,
            Type = eventType.FullName ?? eventType.Name,
            Payload = JsonSerializer.Serialize(integrationEvent, eventType, SerializerOptions),
            OccurredOnUtc = new DateTimeOffset(
                DateTime.SpecifyKind(integrationEvent.OccurredOnUtc, DateTimeKind.Utc)),
        };

        return dbContext.OutboxMessages.AddAsync(message, cancellationToken).AsTask();
    }

    public async Task MarkProcessedAsync(
        Guid eventId,
        CancellationToken cancellationToken = default)
    {
        var message = await FindAsync(eventId, cancellationToken).ConfigureAwait(false);

        if (message is null)
        {
            return;
        }

        message.ProcessedOnUtc = clock.GetUtcNow();
        message.Error = null;

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task MarkFailedAsync(
        Guid eventId,
        string error,
        CancellationToken cancellationToken = default)
    {
        var message = await FindAsync(eventId, cancellationToken).ConfigureAwait(false);

        if (message is null)
        {
            return;
        }

        message.Attempts++;
        message.Error = error.Length <= ErrorMaxLength ? error : error[..ErrorMaxLength];

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private Task<OutboxMessage?> FindAsync(Guid eventId, CancellationToken cancellationToken) =>
        dbContext.OutboxMessages.FirstOrDefaultAsync(message => message.Id == eventId, cancellationToken);
}
