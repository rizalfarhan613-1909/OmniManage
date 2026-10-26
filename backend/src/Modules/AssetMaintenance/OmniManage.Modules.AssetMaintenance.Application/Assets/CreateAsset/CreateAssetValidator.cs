using FluentValidation;
using OmniManage.Modules.AssetMaintenance.Domain.Assets;

namespace OmniManage.Modules.AssetMaintenance.Application.Assets.CreateAsset;

public sealed class CreateAssetValidator : AbstractValidator<CreateAssetCommand>
{
    private const string AssetCodePattern = "^[A-Za-z0-9][A-Za-z0-9._-]*$";

    public CreateAssetValidator()
    {
        RuleFor(command => command.AssetCode)
            .NotEmpty()
            .Length(Asset.AssetCodeMinLength, Asset.AssetCodeMaxLength)
            .Matches(AssetCodePattern)
            .WithMessage(
                "Asset code must be 2 to 50 characters starting with a letter or digit and containing only letters, digits, hyphens, underscores or periods.");

        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(Asset.NameMaxLength);

        RuleFor(command => command.Category)
            .NotEmpty()
            .MaximumLength(Asset.CategoryMaxLength);

        RuleFor(command => command.SerialNumber)
            .MaximumLength(Asset.SerialNumberMaxLength)
            .When(command => !string.IsNullOrWhiteSpace(command.SerialNumber));

        RuleFor(command => command.QrCodeUrl)
            .MaximumLength(Asset.QrCodeUrlMaxLength)
            .When(command => !string.IsNullOrWhiteSpace(command.QrCodeUrl));

        RuleFor(command => command.PurchaseValue)
            .InclusiveBetween(0m, Asset.MaxAmount);

        RuleFor(command => command.AcquisitionDate)
            .NotEmpty();
    }
}
