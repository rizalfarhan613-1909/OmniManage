using OmniManage.BuildingBlocks.Domain;

namespace OmniManage.Modules.Finance.Domain.Errors;

public static class JournalEntryErrors
{
    public static readonly Error InvalidId = new(
        "FINANCE.JOURNAL_ENTRY.INVALID_ID",
        "A journal entry identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidTenantId = new(
        "FINANCE.JOURNAL_ENTRY.INVALID_TENANT_ID",
        "A tenant identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidEntryNumber = new(
        "FINANCE.JOURNAL_ENTRY.INVALID_ENTRY_NUMBER",
        "Journal entry number is required, must not exceed 50 characters and must start with a letter or digit.",
        ErrorType.Validation);

    public static readonly Error InvalidPostedDate = new(
        "FINANCE.JOURNAL_ENTRY.INVALID_POSTED_DATE",
        "A valid posting date is required.",
        ErrorType.Validation);

    public static readonly Error InvalidDescription = new(
        "FINANCE.JOURNAL_ENTRY.INVALID_DESCRIPTION",
        "Journal entry description is required and must not exceed 500 characters.",
        ErrorType.Validation);

    public static readonly Error NoLines = new(
        "FINANCE.JOURNAL_ENTRY.NO_LINES",
        "A journal entry must contain at least two lines.",
        ErrorType.Validation);

    public static readonly Error InvalidLineId = new(
        "FINANCE.JOURNAL_ENTRY.INVALID_LINE_ID",
        "A journal entry line identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidGlAccountId = new(
        "FINANCE.JOURNAL_ENTRY.INVALID_GL_ACCOUNT_ID",
        "A general ledger account is required on every journal entry line.",
        ErrorType.Validation);

    public static readonly Error InvalidLineAmount = new(
        "FINANCE.JOURNAL_ENTRY.INVALID_LINE_AMOUNT",
        "Each line must carry exactly one positive debit or credit within the supported monetary range.",
        ErrorType.Validation);

    public static readonly Error Unbalanced = new(
        "FINANCE.JOURNAL_ENTRY.UNBALANCED",
        "Total debits must equal total credits and be greater than zero.",
        ErrorType.Conflict);

    public static readonly Error InvalidCreatedAt = new(
        "FINANCE.JOURNAL_ENTRY.INVALID_CREATED_AT",
        "A valid creation time is required.",
        ErrorType.Validation);

    public static readonly Error InvalidCreatedBy = new(
        "FINANCE.JOURNAL_ENTRY.INVALID_CREATED_BY",
        "The user creating the journal entry is required.",
        ErrorType.Validation);

    public static readonly Error InvalidDateRange = new(
        "FINANCE.JOURNAL_ENTRY.INVALID_DATE_RANGE",
        "The posting date range is invalid.",
        ErrorType.Validation);

    public static readonly Error PostedEntriesAreImmutable = new(
        "FINANCE.JOURNAL_ENTRY.POSTED_ENTRIES_ARE_IMMUTABLE",
        "Posted journal entries are append-only and cannot be modified or deleted. Post a reversing entry instead.",
        ErrorType.Conflict);

    public static readonly Error NotFound = new(
        "FINANCE.JOURNAL_ENTRY.NOT_FOUND",
        "The journal entry was not found.",
        ErrorType.NotFound);
}
