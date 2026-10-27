namespace OmniManage.Modules.Platform.Presentation.Controllers;

public sealed record UpdateTenantRequest(string? Status);

public sealed record ToggleTenantModuleRequest(bool IsActive, string? Reason);
