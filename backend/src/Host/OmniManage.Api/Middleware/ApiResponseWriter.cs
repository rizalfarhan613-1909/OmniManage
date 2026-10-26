using System.Diagnostics;
using OmniManage.BuildingBlocks.Contracts;

namespace OmniManage.Api.Middleware;

public sealed class ApiResponseWriter(TimeProvider clock) : IApiResponseWriter
{
    public async Task WriteErrorAsync(
        HttpContext context,
        int statusCode,
        string errorCode,
        string message,
        IReadOnlyList<ApiErrorDetail>? details = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json; charset=utf-8";

        var meta = new ApiMeta(
            Activity.Current?.Id ?? context.TraceIdentifier,
            clock.GetUtcNow().UtcDateTime);
        var response = ApiResponse<object>.Fail(new ApiError(errorCode, message, details), meta);

        await context.Response
            .WriteAsJsonAsync(response, cancellationToken)
            .ConfigureAwait(false);
    }
}
