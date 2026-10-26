using System.Globalization;
using OmniManage.Modules.MasterData.Domain.BusinessPartners;

namespace OmniManage.Modules.MasterData.Application.BusinessPartners;

public sealed record BusinessPartnerDto(
    Guid Id,
    string Code,
    string Name,
    string PartnerType,
    string? TaxIdMasked,
    string CreditLimit,
    bool IsActive,
    DateTimeOffset CreatedAt)
{
    internal static BusinessPartnerDto From(BusinessPartner businessPartner)
    {
        ArgumentNullException.ThrowIfNull(businessPartner);

        return new BusinessPartnerDto(
            businessPartner.Id,
            businessPartner.Code,
            businessPartner.Name,
            businessPartner.PartnerType.ToString(),
            MaskTaxId(businessPartner.TaxId),
            businessPartner.CreditLimit.ToString("0.0000", CultureInfo.InvariantCulture),
            businessPartner.IsActive,
            businessPartner.CreatedAt);
    }

    private static string? MaskTaxId(string? taxId) => taxId switch
    {
        null => null,
        { Length: <= 4 } => "****",
        _ => "****" + taxId[^4..],
    };
}
