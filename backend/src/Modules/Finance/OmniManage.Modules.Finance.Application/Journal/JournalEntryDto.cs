using System.Globalization;
using OmniManage.Modules.Finance.Domain.Gl;

namespace OmniManage.Modules.Finance.Application.Journal;

public sealed record JournalEntryDto(
    Guid Id,
    string EntryNumber,
    DateTimeOffset PostedDateUtc,
    string Description,
    string DebitTotal,
    string CreditTotal,
    IReadOnlyList<JournalEntryLineDto> Lines,
    DateTimeOffset CreatedAt)
{
    internal static JournalEntryDto From(JournalEntry journalEntry)
    {
        ArgumentNullException.ThrowIfNull(journalEntry);

        return new JournalEntryDto(
            journalEntry.Id,
            journalEntry.EntryNumber,
            journalEntry.PostedDateUtc,
            journalEntry.Description,
            FormatAmount(journalEntry.DebitTotal),
            FormatAmount(journalEntry.CreditTotal),
            journalEntry.Lines.Select(JournalEntryLineDto.From).ToArray(),
            journalEntry.CreatedAt);
    }

    internal static string FormatAmount(decimal amount) =>
        amount.ToString("0.0000", CultureInfo.InvariantCulture);
}

public sealed record JournalEntryLineDto(
    Guid GlAccountId,
    string Debit,
    string Credit)
{
    internal static JournalEntryLineDto From(JournalEntryLine line)
    {
        ArgumentNullException.ThrowIfNull(line);

        return new JournalEntryLineDto(
            line.GlAccountId,
            JournalEntryDto.FormatAmount(line.Debit),
            JournalEntryDto.FormatAmount(line.Credit));
    }
}
