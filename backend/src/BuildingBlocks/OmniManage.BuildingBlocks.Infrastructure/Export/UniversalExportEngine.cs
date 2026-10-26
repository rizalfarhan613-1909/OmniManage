using System.Collections.Concurrent;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text;
using HTMLQuestPDF.Extensions;
using MiniExcelLibs;
using OmniManage.BuildingBlocks.Application;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace OmniManage.BuildingBlocks.Infrastructure.Export;

/// <summary>
/// Generates PDF documents from HTML and streaming Excel workbooks from typed rows.
/// </summary>
public sealed class UniversalExportEngine : IExportEngine
{
    private const int MaxWorksheetNameLength = 31;

    private static readonly char[] InvalidWorksheetNameCharacters = ['\\', '/', '?', '*', '[', ']', ':'];

    private static readonly ConcurrentDictionary<Type, ExportColumn[]> ColumnCache = new();

    static UniversalExportEngine()
    {
        // QuestPDF requires an explicit license selection before the first document is
        // generated. OmniManage uses the free Community license.
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public Task<Stream> GeneratePdfAsync(string htmlContent, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(htmlContent);
        ct.ThrowIfCancellationRequested();

        var stream = TemporaryFileStream.Create();

        try
        {
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(15, Unit.Millimetre);
                    page.DefaultTextStyle(style => style.FontSize(9));

                    page.Content().HTML(descriptor => descriptor.SetHtml(htmlContent));

                    page.Footer().AlignRight().Text(text =>
                    {
                        text.DefaultTextStyle(style => style.FontSize(8).FontColor(Colors.Grey.Medium));
                        text.CurrentPageNumber();
                        text.Span(" / ");
                        text.TotalPages();
                    });
                });
            }).GeneratePdf(stream);

            stream.Position = 0;

            return Task.FromResult<Stream>(stream);
        }
        catch
        {
            stream.Dispose();
            throw;
        }
    }

    public Task<Stream> GenerateExcelAsync<T>(
        IReadOnlyCollection<T> rows,
        string worksheetName,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(rows);
        ArgumentException.ThrowIfNullOrWhiteSpace(worksheetName);
        ct.ThrowIfCancellationRequested();

        var columns = ColumnCache.GetOrAdd(typeof(T), static type => BuildColumns(type));
        var sheetName = NormalizeWorksheetName(worksheetName);
        var stream = TemporaryFileStream.Create();

        try
        {
            stream.SaveAs(
                ProjectRows(rows, columns, ct),
                printHeader: true,
                sheetName: sheetName,
                excelType: ExcelType.XLSX);

            stream.Position = 0;

            return Task.FromResult<Stream>(stream);
        }
        catch
        {
            stream.Dispose();
            throw;
        }
    }

    private static IEnumerable<Dictionary<string, object?>> ProjectRows<T>(
        IReadOnlyCollection<T> rows,
        IReadOnlyList<ExportColumn> columns,
        CancellationToken ct)
    {
        foreach (var row in rows)
        {
            ct.ThrowIfCancellationRequested();

            if (row is null)
            {
                throw new InvalidOperationException("Export rows cannot contain null values.");
            }

            var record = new Dictionary<string, object?>(columns.Count, StringComparer.Ordinal);

            foreach (var column in columns)
            {
                record[column.Header] = ConvertValue(column.Property.GetValue(row));
            }

            yield return record;
        }
    }

    private static object? ConvertValue(object? value) =>
        value switch
        {
            null => null,
            string text => text,
            bool boolean => boolean,
            DateTime dateTime => dateTime,
            DateTimeOffset dateTimeOffset => dateTimeOffset.DateTime,
            DateOnly date => date.ToDateTime(TimeOnly.MinValue),
            TimeOnly time => time.ToString("HH:mm:ss", CultureInfo.InvariantCulture),
            Guid guid => guid.ToString("D", CultureInfo.InvariantCulture),
            byte[] bytes => Convert.ToBase64String(bytes),
            char character => character.ToString(),
            Enum enumeration => enumeration.ToString(),
            byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal => value,
            _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty,
        };

    private static ExportColumn[] BuildColumns(Type type)
    {
        var properties = type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property => property.CanRead &&
                               property.GetMethod is { IsPublic: true } &&
                               property.GetIndexParameters().Length == 0)
            .OrderBy(property => property.MetadataToken)
            .ToArray();

        if (properties.Length == 0)
        {
            throw new InvalidOperationException(
                $"Type '{type.FullName}' does not expose any readable public properties to export.");
        }

        var headerCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var columns = new List<ExportColumn>(properties.Length);

        foreach (var property in properties)
        {
            var header = GetHeader(property);

            if (headerCounts.TryGetValue(header, out var count))
            {
                count++;
                headerCounts[header] = count;
                header = $"{header} ({count})";
            }
            else
            {
                headerCounts[header] = 1;
            }

            columns.Add(new ExportColumn(property, header));
        }

        return columns.ToArray();
    }

    private static string GetHeader(PropertyInfo property)
    {
        var displayName = property.GetCustomAttribute<DisplayNameAttribute>();

        if (displayName is not null && !string.IsNullOrWhiteSpace(displayName.DisplayName))
        {
            return displayName.DisplayName.Trim();
        }

        var builder = new StringBuilder(property.Name.Length + 4);

        for (var index = 0; index < property.Name.Length; index++)
        {
            var character = property.Name[index];

            if (index > 0 &&
                char.IsUpper(character) &&
                (char.IsLower(property.Name[index - 1]) ||
                 (index + 1 < property.Name.Length && char.IsLower(property.Name[index + 1]))))
            {
                builder.Append(' ');
            }

            builder.Append(character);
        }

        return builder.ToString();
    }

    private static string NormalizeWorksheetName(string worksheetName)
    {
        var builder = new StringBuilder(worksheetName.Length);

        foreach (var character in worksheetName.Trim().Trim('\''))
        {
            builder.Append(InvalidWorksheetNameCharacters.Contains(character) ? ' ' : character);
        }

        var normalized = builder.ToString().Trim();

        if (normalized.Length == 0)
        {
            return "Sheet1";
        }

        return normalized.Length <= MaxWorksheetNameLength
            ? normalized
            : normalized[..MaxWorksheetNameLength];
    }

    private sealed record ExportColumn(PropertyInfo Property, string Header);
}
