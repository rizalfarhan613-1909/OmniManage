using OmniManage.BuildingBlocks.Application;
using OmniManage.Modules.Platform.Application.Admin.Dtos;

namespace OmniManage.Modules.Platform.Application.Admin.Commands;

public sealed record UpdateTenantStatusCommand(
    Guid TenantId,
    string? Status) : ICommand<TenantStatusDto>;
