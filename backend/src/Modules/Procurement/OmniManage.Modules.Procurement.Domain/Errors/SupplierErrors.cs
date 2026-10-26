using OmniManage.BuildingBlocks.Domain;

namespace OmniManage.Modules.Procurement.Domain.Errors;

public static class SupplierErrors
{
    public static readonly Error InvalidId = new(
        "PROCUREMENT.SUPPLIER.INVALID_ID",
        "A supplier identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidTenantId = new(
        "PROCUREMENT.SUPPLIER.INVALID_TENANT_ID",
        "A tenant identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidCode = new(
        "PROCUREMENT.SUPPLIER.INVALID_CODE",
        "Supplier code is out of range or uses unsupported characters.",
        ErrorType.Validation);

    public static readonly Error InvalidName = new(
        "PROCUREMENT.SUPPLIER.INVALID_NAME",
        "Supplier name is blank or longer than 200 characters.",
        ErrorType.Validation);

    public static readonly Error InvalidContactName = new(
        "PROCUREMENT.SUPPLIER.INVALID_CONTACT_NAME",
        "Supplier contact name is longer than 100 characters.",
        ErrorType.Validation);

    public static readonly Error InvalidEmail = new(
        "PROCUREMENT.SUPPLIER.INVALID_EMAIL",
        "Supplier email is malformed or longer than 320 characters.",
        ErrorType.Validation);

    public static readonly Error InvalidPhone = new(
        "PROCUREMENT.SUPPLIER.INVALID_PHONE",
        "Supplier phone number is longer than 50 characters.",
        ErrorType.Validation);

    public static readonly Error InvalidPaymentTermsDays = new(
        "PROCUREMENT.SUPPLIER.INVALID_PAYMENT_TERMS_DAYS",
        "Payment terms must be between 0 and 365 days.",
        ErrorType.Validation);

    public static readonly Error InvalidCreatedAt = new(
        "PROCUREMENT.SUPPLIER.INVALID_CREATED_AT",
        "A valid creation time is required.",
        ErrorType.Validation);

    public static readonly Error InvalidCreatedBy = new(
        "PROCUREMENT.SUPPLIER.INVALID_CREATED_BY",
        "The user creating the supplier is required.",
        ErrorType.Validation);

    public static readonly Error CodeAlreadyExists = new(
        "PROCUREMENT.SUPPLIER.CODE_ALREADY_EXISTS",
        "A supplier with this code already exists.",
        ErrorType.Conflict);

    public static readonly Error Inactive = new(
        "PROCUREMENT.SUPPLIER.INACTIVE",
        "Purchase orders cannot be placed with an inactive supplier.",
        ErrorType.Conflict);

    public static readonly Error NotFound = new(
        "PROCUREMENT.SUPPLIER.NOT_FOUND",
        "The supplier was not found.",
        ErrorType.NotFound);
}
