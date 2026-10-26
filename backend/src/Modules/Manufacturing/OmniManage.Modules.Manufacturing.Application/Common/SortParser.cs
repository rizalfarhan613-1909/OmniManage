namespace OmniManage.Modules.Manufacturing.Application.Common;

internal sealed record SortOption<TSortField>(TSortField Field, bool Descending)
    where TSortField : struct, Enum;

internal static class SortParser
{
    private const int MaxSortFields = 3;

    public static IReadOnlyList<SortOption<TSortField>> Parse<TSortField>(
        string? sort,
        IReadOnlyDictionary<string, TSortField> allowedFields,
        TSortField defaultField)
        where TSortField : struct, Enum
    {
        ArgumentNullException.ThrowIfNull(allowedFields);

        if (string.IsNullOrWhiteSpace(sort))
        {
            return [new SortOption<TSortField>(defaultField, false)];
        }

        var options = new List<SortOption<TSortField>>();

        foreach (var entry in sort.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (options.Count == MaxSortFields)
            {
                break;
            }

            var descending = entry.StartsWith('-');
            var fieldName = descending ? entry[1..] : entry;

            if (allowedFields.TryGetValue(fieldName, out var field))
            {
                options.Add(new SortOption<TSortField>(field, descending));
            }
        }

        return options.Count == 0
            ? [new SortOption<TSortField>(defaultField, false)]
            : options;
    }
}
