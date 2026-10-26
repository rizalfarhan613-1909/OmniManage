using System.Text.Json;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.BuildingBlocks.Infrastructure.Persistence.Outbox;
using OmniManage.Modules.Finance.Application.IntegrationEvents;
using OmniManage.Modules.Finance.Infrastructure.Persistence;

namespace OmniManage.Modules.Finance.Infrastructure.Outbox;

public sealed class FinanceOutbox(FinanceDbContext dbContext) : IFinanceOutbox
{
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
}
