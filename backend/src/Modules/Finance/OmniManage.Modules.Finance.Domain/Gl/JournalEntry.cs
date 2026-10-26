using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Finance.Domain.Errors;

namespace OmniManage.Modules.Finance.Domain.Gl;

/// <summary>
/// An immutable, balanced double-entry posting. Posted entries are never
/// updated or deleted; corrections are made with a reversing entry.
/// </summary>
public sealed class JournalEntry : AggregateRoot<Guid>, ITenantEntity, IAuditable
{
    public const int EntryNumberMaxLength = 50;
    public const int DescriptionMaxLength = 500;
    public const decimal MaxAmount = 999_999_999_999_999.9999m;

    private readonly List<JournalEntryLine> _lines = [];

    private JournalEntry()
    {
    }

    private JournalEntry(
        Guid id,
        Guid tenantId,
        string entryNumber,
        DateTimeOffset postedDateUtc,
        string description,
        DateTimeOffset createdAt,
        Guid createdBy)
        : base(id)
    {
        TenantId = tenantId;
        EntryNumber = entryNumber;
        PostedDateUtc = postedDateUtc;
        Description = description;
        CreatedAt = createdAt;
        CreatedBy = createdBy;
    }

    public Guid TenantId { get; private set; }

    public string EntryNumber { get; private set; } = string.Empty;

    public DateTimeOffset PostedDateUtc { get; private set; }

    public string Description { get; private set; } = string.Empty;

    public IReadOnlyList<JournalEntryLine> Lines => _lines;

    public decimal DebitTotal => _lines.Sum(line => line.Debit);

    public decimal CreditTotal => _lines.Sum(line => line.Credit);

    public DateTimeOffset CreatedAt { get; private set; }

    public Guid CreatedBy { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public Guid? UpdatedBy { get; private set; }

    public static Result<JournalEntry> Create(
        Guid id,
        Guid tenantId,
        string? entryNumber,
        DateTimeOffset postedDateUtc,
        string? description,
        IReadOnlyList<JournalEntryLineInput>? lines,
        DateTimeOffset createdAt,
        Guid createdBy)
    {
        if (id == Guid.Empty)
        {
            return JournalEntryErrors.InvalidId;
        }

        if (tenantId == Guid.Empty)
        {
            return JournalEntryErrors.InvalidTenantId;
        }

        if (!TryNormalizeEntryNumber(entryNumber, out var normalizedEntryNumber))
        {
            return JournalEntryErrors.InvalidEntryNumber;
        }

        if (postedDateUtc == default)
        {
            return JournalEntryErrors.InvalidPostedDate;
        }

        if (!TryNormalizeDescription(description, out var normalizedDescription))
        {
            return JournalEntryErrors.InvalidDescription;
        }

        if (lines is null || lines.Count < 2)
        {
            return JournalEntryErrors.NoLines;
        }

        if (createdAt == default)
        {
            return JournalEntryErrors.InvalidCreatedAt;
        }

        if (createdBy == Guid.Empty)
        {
            return JournalEntryErrors.InvalidCreatedBy;
        }

        var entry = new JournalEntry(
            id,
            tenantId,
            normalizedEntryNumber,
            postedDateUtc,
            normalizedDescription,
            createdAt,
            createdBy);

        var debitTotal = 0m;
        var creditTotal = 0m;

        foreach (var lineInput in lines)
        {
            var line = JournalEntryLine.Create(
                lineInput.LineId,
                entry.Id,
                lineInput.GlAccountId,
                lineInput.Debit,
                lineInput.Credit);

            if (line.IsFailure)
            {
                return line.Error;
            }

            try
            {
                debitTotal = decimal.Round(
                    debitTotal + line.Value.Debit,
                    4,
                    MidpointRounding.AwayFromZero);
                creditTotal = decimal.Round(
                    creditTotal + line.Value.Credit,
                    4,
                    MidpointRounding.AwayFromZero);
            }
            catch (OverflowException)
            {
                return JournalEntryErrors.InvalidLineAmount;
            }

            entry._lines.Add(line.Value);
        }

        if (debitTotal > MaxAmount || creditTotal > MaxAmount)
        {
            return JournalEntryErrors.InvalidLineAmount;
        }

        if (debitTotal != creditTotal || debitTotal == 0m)
        {
            return JournalEntryErrors.Unbalanced;
        }

        return entry;
    }

    private static bool TryNormalizeEntryNumber(string? value, out string normalized)
    {
        normalized = string.Empty;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var candidate = value.Trim().ToUpperInvariant();

        if (candidate.Length > EntryNumberMaxLength || !char.IsAsciiLetterOrDigit(candidate[0]))
        {
            return false;
        }

        foreach (var character in candidate)
        {
            if (!char.IsAsciiLetterOrDigit(character) && character is not ('-' or '_' or '.'))
            {
                return false;
            }
        }

        normalized = candidate;
        return true;
    }

    private static bool TryNormalizeDescription(string? value, out string normalized)
    {
        normalized = string.Empty;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var candidate = value.Trim();

        if (candidate.Length > DescriptionMaxLength)
        {
            return false;
        }

        normalized = candidate;
        return true;
    }
}
