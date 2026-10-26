using OmniManage.BuildingBlocks.Application;

namespace OmniManage.Modules.MasterData.Application.BusinessPartners.CreateBusinessPartner;

public sealed record CreateBusinessPartnerCommand(
    string? Code,
    string? Name,
    string? PartnerType,
    string? TaxId,
    decimal CreditLimit) : ICommand<BusinessPartnerDto>;
