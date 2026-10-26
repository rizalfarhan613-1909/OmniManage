namespace OmniManage.BuildingBlocks.Contracts;

public sealed record ApiError(string Code, string Message, IReadOnlyList<ApiErrorDetail>? Details = null);
