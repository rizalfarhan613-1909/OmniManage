using OmniManage.BuildingBlocks.Domain;

namespace OmniManage.Modules.Finance.Domain.Errors;

public static class GlAccountErrors
{
    public static readonly Error InvalidId = new(
        "FINANCE.GL_ACCOUNT.INVALID_ID",
        "A general ledger account identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidTenantId = new(
        "FINANCE.GL_ACCOUNT.INVALID_TENANT_ID",
        "A tenant identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidCode = new(
        "FINANCE.GL_ACCOUNT.INVALID_CODE",
        "Account code is required, must not exceed 20 characters and must start with a letter or digit.",
        ErrorType.Validation);

    public static readonly Error InvalidName = new(
        "FINANCE.GL_ACCOUNT.INVALID_NAME",
        "Account name is required and must not exceed 200 characters.",
        ErrorType.Validation);

    public static readonly Error InvalidType = new(
        "FINANCE.GL_ACCOUNT.INVALID_TYPE",
        "Account type must be Asset, Liability, Equity, Revenue or Expense.",
        ErrorType.Validation);

    public static readonly Error InvalidCreatedAt = new(
        "FINANCE.GL_ACCOUNT.INVALID_CREATED_AT",
        "A valid creation time is required.",
        ErrorType.Validation);

    public static readonly Error InvalidCreatedBy = new(
        "FINANCE.GL_ACCOUNT.INVALID_CREATED_BY",
        "The user creating the account is required.",
        ErrorType.Validation);
}
