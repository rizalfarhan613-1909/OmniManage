using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Domain;

namespace OmniManage.Modules.Platform.Application.Auth;

public sealed record GetSessionQuery : IQuery<Result<AuthSessionDto>>;
