using OmniManage.BuildingBlocks.Application;
using OmniManage.Modules.Platform.Application.Admin.Dtos;

namespace OmniManage.Modules.Platform.Application.Admin.Commands;

public sealed record ToggleTenantModuleCommand(
    Guid TenantId,
    string? ModuleCode,
    bool IsActive,
    string? Reason) : ICommand<TenantModuleLicenseDto>;
