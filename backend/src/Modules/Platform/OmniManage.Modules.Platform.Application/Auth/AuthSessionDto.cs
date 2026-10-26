namespace OmniManage.Modules.Platform.Application.Auth;

public sealed record AuthSessionDto(
    AuthUserDto User,
    AuthTenantDto Tenant,
    IReadOnlyList<string> ActiveModules,
    IReadOnlyList<string> Permissions);
