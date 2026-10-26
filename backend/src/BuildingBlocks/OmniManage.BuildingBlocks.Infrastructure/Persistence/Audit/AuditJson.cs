using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OmniManage.BuildingBlocks.Infrastructure.Persistence.Audit;

public static class AuditJson
{
    public const string RedactedValue = "[REDACTED]";

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public static string? Serialize(object? value)
    {
        if (value is null)
        {
            return null;
        }

        return JsonSerializer.Serialize(Redact(value), Options);
    }

    public static bool IsSensitiveName(string propertyName) =>
        propertyName.Contains("password", StringComparison.OrdinalIgnoreCase)
        || propertyName.Contains("secret", StringComparison.OrdinalIgnoreCase)
        || propertyName.Contains("token", StringComparison.OrdinalIgnoreCase)
        || propertyName.Contains("bank", StringComparison.OrdinalIgnoreCase)
        || propertyName.Contains("nationalid", StringComparison.OrdinalIgnoreCase)
        || propertyName.Contains("taxid", StringComparison.OrdinalIgnoreCase);

    private static object? Redact(object? value)
    {
        if (value is null)
        {
            return null;
        }

        if (value is IDictionary<string, object?> dictionary)
        {
            var redacted = new Dictionary<string, object?>(StringComparer.Ordinal);

            foreach (var pair in dictionary.OrderBy(item => item.Key, StringComparer.Ordinal))
            {
                redacted[pair.Key] = IsSensitiveName(pair.Key)
                    ? RedactedValue
                    : Redact(pair.Value);
            }

            return redacted;
        }

        if (value is IEnumerable<KeyValuePair<string, object?>> pairs)
        {
            return Redact(pairs.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal));
        }

        return value;
    }
}
