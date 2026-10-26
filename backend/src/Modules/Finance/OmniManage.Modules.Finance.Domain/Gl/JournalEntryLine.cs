using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Finance.Domain.Errors;

namespace OmniManage.Modules.Finance.Domain.Gl;

/// <summary>
/// A single debit or credit line of a posted journal entry. Lines are part of
/// the <see cref="JournalEntry"/> aggregate and never modified after posting.
/// </summary>
public sealed class JournalEntryLine : Entity<Guid>
{
    private JournalEntryLine()
    {
    }

    private JournalEntryLine(
        Guid id,
        Guid journalEntryId,
        Guid glAccountId,
        decimal debit,
        decimal credit)
        : base(id)
    {
        JournalEntryId = journalEntryId;
        GlAccountId = glAccountId;
        Debit = debit;
        Credit = credit;
    }

    public Guid JournalEntryId { get; private set; }

    public Guid GlAccountId { get; private set; }

    public decimal Debit { get; private set; }

    public decimal Credit { get; private set; }

    internal static Result<JournalEntryLine> Create(
        Guid id,
        Guid journalEntryId,
        Guid glAccountId,
        decimal debit,
        decimal credit)
    {
        if (id == Guid.Empty)
        {
            return JournalEntryErrors.InvalidLineId;
        }

        if (journalEntryId == Guid.Empty)
        {
            return JournalEntryErrors.InvalidId;
        }

        if (glAccountId == Guid.Empty)
        {
            return JournalEntryErrors.InvalidGlAccountId;
        }

        if (debit < 0m || credit < 0m)
        {
            return JournalEntryErrors.InvalidLineAmount;
        }

        if (debit == 0m && credit == 0m)
        {
            return JournalEntryErrors.InvalidLineAmount;
        }

        if (debit > 0m && credit > 0m)
        {
            return JournalEntryErrors.InvalidLineAmount;
        }

        return new JournalEntryLine(id, journalEntryId, glAccountId, debit, credit);
    }
}
