using System.Text.Json;
using OmniManage.Modules.Platform.Domain.Abstractions;

namespace OmniManage.Modules.Platform.Application.Admin.Dtos;

public sealed record AuditActorDto(Guid? Id, string? Name, string Type);

public sealed record AuditLogEntryDto(
    Guid Id,
    DateTimeOffset OccurredAt,
    AuditActorDto Actor,
    string Action,
    string? ModuleCode,
    string? EntityType,
    string? EntityId,
    string? IpAddress,
    string? UserAgent,
    string TraceId,
    JsonElement? BeforeData,
    JsonElement? AfterData,
    string RowHash,
    string? PrevHash)
{
    internal static AuditLogEntryDto From(AuditLogProjection projection)
    {
        ArgumentNullException.ThrowIfNull(projection);

        return new AuditLogEntryDto(
            projection.Id,
            projection.OccurredAt,
            new AuditActorDto(projection.ActorId, projection.ActorName, projection.ActorType),
            projection.Action,
            projection.ModuleCode,
            projection.EntityType,
            projection.EntityId,
            projection.IpAddress,
            projection.UserAgent,
            projection.TraceId ?? string.Empty,
            ParseJson(projection.BeforeData),
            ParseJson(projection.AfterData),
            projection.RowHash is { Length: > 0 }
                ? Convert.ToHexString(projection.RowHash).ToLowerInvariant()
                : string.Empty,
            projection.PrevHash is { Length: > 0 }
                ? Convert.ToHexString(projection.PrevHash).ToLowerInvariant()
                : null);
    }

    private static JsonElement? ParseJson(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(value);

            return document.RootElement.Clone();
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
