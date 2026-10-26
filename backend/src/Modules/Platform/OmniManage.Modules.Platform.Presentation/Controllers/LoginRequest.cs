namespace OmniManage.Modules.Platform.Presentation.Controllers;

public sealed record LoginRequest(string? TenantCode, string? Email, string? Password);
