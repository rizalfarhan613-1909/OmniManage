namespace OmniManage.Modules.MasterData.Presentation.Controllers;

public sealed record CreateBusinessPartnerRequest(
    string? Code,
    string? Name,
    string? PartnerType,
    string? TaxId,
    decimal CreditLimit);
