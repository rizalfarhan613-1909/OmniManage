using OmniManage.BuildingBlocks.Domain;

namespace OmniManage.Modules.MasterData.Domain.Errors;

public static class BusinessPartnerErrors
{
    public static readonly Error InvalidId = new(
        "MASTERDATA.BUSINESS_PARTNER.INVALID_ID",
        "A business partner identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidTenantId = new(
        "MASTERDATA.BUSINESS_PARTNER.INVALID_TENANT_ID",
        "A tenant identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidCode = new(
        "MASTERDATA.BUSINESS_PARTNER.INVALID_CODE",
        "Business partner code must be 2 to 50 characters starting with a letter or digit and containing only letters, digits, hyphens or underscores.",
        ErrorType.Validation);

    public static readonly Error InvalidName = new(
        "MASTERDATA.BUSINESS_PARTNER.INVALID_NAME",
        "Business partner name is required and must not exceed 200 characters.",
        ErrorType.Validation);

    public static readonly Error InvalidPartnerType = new(
        "MASTERDATA.BUSINESS_PARTNER.INVALID_PARTNER_TYPE",
        "Partner type must be Customer, Vendor or Both.",
        ErrorType.Validation);

    public static readonly Error InvalidTaxId = new(
        "MASTERDATA.BUSINESS_PARTNER.INVALID_TAX_ID",
        "Tax identifier must not exceed 50 characters.",
        ErrorType.Validation);

    public static readonly Error InvalidCreditLimit = new(
        "MASTERDATA.BUSINESS_PARTNER.INVALID_CREDIT_LIMIT",
        "Credit limit must be between 0 and 999,999,999,999,999.9999.",
        ErrorType.Validation);

    public static readonly Error InvalidCreatedAt = new(
        "MASTERDATA.BUSINESS_PARTNER.INVALID_CREATED_AT",
        "A valid creation time is required.",
        ErrorType.Validation);

    public static readonly Error InvalidCreatedBy = new(
        "MASTERDATA.BUSINESS_PARTNER.INVALID_CREATED_BY",
        "The user creating the business partner is required.",
        ErrorType.Validation);

    public static readonly Error CodeAlreadyExists = new(
        "MASTERDATA.BUSINESS_PARTNER.CODE_ALREADY_EXISTS",
        "A business partner with this code already exists.",
        ErrorType.Conflict);
}
