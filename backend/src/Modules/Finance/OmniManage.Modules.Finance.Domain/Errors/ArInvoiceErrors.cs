using OmniManage.BuildingBlocks.Domain;

namespace OmniManage.Modules.Finance.Domain.Errors;

public static class ArInvoiceErrors
{
    public static readonly Error InvalidId = new(
        "FINANCE.AR_INVOICE.INVALID_ID",
        "An A/R invoice identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidTenantId = new(
        "FINANCE.AR_INVOICE.INVALID_TENANT_ID",
        "A tenant identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidInvoiceNumber = new(
        "FINANCE.AR_INVOICE.INVALID_INVOICE_NUMBER",
        "Invoice number is required, must not exceed 50 characters and must start with a letter or digit.",
        ErrorType.Validation);

    public static readonly Error InvalidSalesOrderId = new(
        "FINANCE.AR_INVOICE.INVALID_SALES_ORDER_ID",
        "A sales order identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidCustomerId = new(
        "FINANCE.AR_INVOICE.INVALID_CUSTOMER_ID",
        "A customer identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidTotalAmount = new(
        "FINANCE.AR_INVOICE.INVALID_TOTAL_AMOUNT",
        "Invoice total must be greater than zero and within the supported monetary range.",
        ErrorType.Validation);

    public static readonly Error InvalidCreatedAt = new(
        "FINANCE.AR_INVOICE.INVALID_CREATED_AT",
        "A valid creation time is required.",
        ErrorType.Validation);

    public static readonly Error InvalidCreatedBy = new(
        "FINANCE.AR_INVOICE.INVALID_CREATED_BY",
        "The user creating the invoice is required.",
        ErrorType.Validation);

    public static readonly Error InvalidStatusFilter = new(
        "FINANCE.AR_INVOICE.INVALID_STATUS_FILTER",
        "Status must be Unpaid, Paid or Cancelled.",
        ErrorType.Validation);

    public static readonly Error InvalidStatusTransition = new(
        "FINANCE.AR_INVOICE.INVALID_STATUS_TRANSITION",
        "Only an unpaid invoice can be marked as paid.",
        ErrorType.Conflict);

    public static readonly Error NotFound = new(
        "FINANCE.AR_INVOICE.NOT_FOUND",
        "The A/R invoice was not found.",
        ErrorType.NotFound);
}
