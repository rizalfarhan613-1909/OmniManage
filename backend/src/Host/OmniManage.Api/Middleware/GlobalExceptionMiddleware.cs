using OmniManage.BuildingBlocks.Domain;

namespace OmniManage.Api.Middleware;

/// <summary>
/// Converts unexpected exceptions to the standard <c>ApiResponse</c> envelope
/// (Rules.md §6.2). Expected business failures are returned as <c>Result</c>
/// values by handlers and never reach this middleware.
/// </summary>
public sealed class GlobalExceptionMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionMiddleware> logger)
{
    private const int ClientClosedRequestStatusCode = 499;

    public async Task InvokeAsync(HttpContext context, IApiResponseWriter responseWriter)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(responseWriter);

        try
        {
            await next(context).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // The browser navigated away or aborted the request. Do not log an
            // error or try to write a body to a disconnected client.
            logger.LogDebug(
                "Request {TraceId} was aborted by the client.",
                context.TraceIdentifier);

            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = ClientClosedRequestStatusCode;
            }
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Unhandled exception for request {TraceId}.",
                context.TraceIdentifier);

            if (context.Response.HasStarted)
            {
                throw;
            }

            await responseWriter
                .WriteErrorAsync(
                    context,
                    StatusCodes.Status500InternalServerError,
                    CommonErrors.Unexpected.Code,
                    CommonErrors.Unexpected.Message,
                    cancellationToken: CancellationToken.None)
                .ConfigureAwait(false);
        }
    }
}
