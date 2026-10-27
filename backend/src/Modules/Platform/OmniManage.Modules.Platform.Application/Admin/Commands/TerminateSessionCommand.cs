using OmniManage.BuildingBlocks.Application;
using OmniManage.Modules.Platform.Application.Admin.Dtos;

namespace OmniManage.Modules.Platform.Application.Admin.Commands;

public sealed record TerminateSessionCommand(Guid SessionId) : ICommand<TerminateSessionResultDto>;
