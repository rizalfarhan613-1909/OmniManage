using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.MasterData.Domain.Abstractions;
using OmniManage.Modules.MasterData.Domain.BusinessPartners;
using OmniManage.Modules.MasterData.Domain.Errors;

namespace OmniManage.Modules.MasterData.Application.BusinessPartners.CreateBusinessPartner;

public sealed class CreateBusinessPartnerCommandHandler(
    IBusinessPartnerRepository businessPartners,
    ITenantContext tenantContext,
    ICurrentUser currentUser,
    IGuidGenerator guidGenerator,
    TimeProvider clock)
    : ICommandHandler<CreateBusinessPartnerCommand, BusinessPartnerDto>
{
    public async Task<Result<BusinessPartnerDto>> Handle(
        CreateBusinessPartnerCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var created = BusinessPartner.Create(
            guidGenerator.NewGuid(),
            tenantContext.TenantId,
            command.Code,
            command.Name,
            command.PartnerType,
            command.TaxId,
            command.CreditLimit,
            clock.GetUtcNow(),
            currentUser.UserId);

        if (created.IsFailure)
        {
            return created.Error;
        }

        var businessPartner = created.Value;

        if (await businessPartners.CodeExistsAsync(businessPartner.Code, cancellationToken).ConfigureAwait(false))
        {
            return BusinessPartnerErrors.CodeAlreadyExists;
        }

        await businessPartners.AddAsync(businessPartner, cancellationToken).ConfigureAwait(false);
        await businessPartners.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return BusinessPartnerDto.From(businessPartner);
    }
}
