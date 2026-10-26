namespace OmniManage.BuildingBlocks.Contracts;

public sealed record ApiErrorDetail(string? Field, string Code, string Message);
