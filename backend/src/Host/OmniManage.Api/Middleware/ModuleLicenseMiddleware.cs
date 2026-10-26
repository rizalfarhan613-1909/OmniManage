using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.BuildingBlocks.Contracts.Security;
using OmniManage.Modules.Platform.Domain.Errors;

namespace OmniManage.Api.Middleware;

public sealed class ModuleLicenseMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        ITenantContext tenantContext,
        ITenantModuleCache licenses,
        IApiResponseWriter responseWriter,
        ILogger<ModuleLicenseMiddleware> logger)
    {
        ArgumentNullException.ThrowIfNull(context);

        var endpoint = context.GetEndpoint();

        if (endpoint?.Metadata.GetMetadata<AllowWithoutModuleAttribute>() is not null)
        {
            await next(context).ConfigureAwait(false);
            return;
        }

        var requirements = endpoint?.Metadata.GetOrderedMetadata<RequireModuleAttribute>();

        if (requirements is null || requirements.Count == 0)
        {
            await next(context).ConfigureAwait(false);
            return;
        }

        if (context.User.Identity?.IsAuthenticated != true)
        {
            await next(context).ConfigureAwait(false);
            return;
        }

        LicenseSnapshot licensesSnapshot;

        try
        {
            licensesSnapshot = await licenses
                .GetAsync(tenantContext.TenantId, context.RequestAborted)
                .ConfigureAwait(false);
        }
        catch (LicenseSourceUnavailableException exception)
        {
            logger.LogError(
                exception,
                "Module license source unavailable for tenant {TenantId}",
                tenantContext.TenantId);

            await responseWriter
                .WriteErrorAsync(
                    context,
                    StatusCodes.Status503ServiceUnavailable,
                    LicensingErrors.LicenseUnavailable.Code,
                    LicensingErrors.LicenseUnavailable.Message,
                    cancellationToken: context.RequestAborted)
                .ConfigureAwait(false);
            return;
        }

        foreach (var requirement in requirements)
        {
            if (licensesSnapshot.IsActive(requirement.ModuleCode))
            {
                continue;
            }

            logger.LogInformation(
                "Module license denied for tenant {TenantId}, module {ModuleCode}, user {UserId}",
                tenantContext.TenantId,
                requirement.ModuleCode,
                context.User.FindFirst("sub")?.Value);

            await responseWriter
                .WriteErrorAsync(
                    context,
                    StatusCodes.Status403Forbidden,
                    LicensingErrors.ModuleNotLicensed.Code,
                    $"Module '{requirement.ModuleCode}' is not active for this organization.",
                    [
                        new ApiErrorDetail(
                            "moduleCode",
                            LicensingErrors.ModuleNotLicensed.Code,
                            requirement.ModuleCode),
                    ],
                    context.RequestAborted)
                .ConfigureAwait(false);
            return;
        }

        await next(context).ConfigureAwait(false);
    }
}
