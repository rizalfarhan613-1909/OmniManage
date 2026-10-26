namespace OmniManage.BuildingBlocks.Contracts;

public sealed record ApiResponse<T>(bool Success, T? Data, ApiError? Error, ApiMeta Meta)
{
    public static ApiResponse<T> Ok(T data, ApiMeta meta)
    {
        ArgumentNullException.ThrowIfNull(meta);
        return new ApiResponse<T>(true, data, null, meta);
    }

    public static ApiResponse<T> Fail(ApiError error, ApiMeta meta)
    {
        ArgumentNullException.ThrowIfNull(error);
        ArgumentNullException.ThrowIfNull(meta);
        return new ApiResponse<T>(false, default, error, meta);
    }
}
