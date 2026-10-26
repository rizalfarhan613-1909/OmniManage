using OmniManage.BuildingBlocks.Contracts;

namespace OmniManage.Api.Middleware;

public interface IApiResponseWriter
{
    Task WriteErrorAsync(
        HttpContext context,
        int statusCode,
        string errorCode,
        string message,
        IReadOnlyList<ApiErrorDetail>? details = null,
        CancellationToken cancellationToken = default);
}
