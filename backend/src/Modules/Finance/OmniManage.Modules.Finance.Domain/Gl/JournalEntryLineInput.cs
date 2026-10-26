namespace OmniManage.Modules.Finance.Domain.Gl;

public sealed record JournalEntryLineInput(
    Guid LineId,
    Guid GlAccountId,
    decimal Debit,
    decimal Credit);
