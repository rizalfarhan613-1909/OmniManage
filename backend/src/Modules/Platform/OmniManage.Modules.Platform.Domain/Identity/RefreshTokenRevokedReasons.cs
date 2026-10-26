namespace OmniManage.Modules.Platform.Domain.Identity;

public static class RefreshTokenRevokedReasons
{
    public const string Rotated = "ROTATED";
    public const string Logout = "LOGOUT";
    public const string ReuseDetected = "REUSE_DETECTED";
    public const string Expired = "EXPIRED";
    public const string UserDisabled = "USER_DISABLED";
}
