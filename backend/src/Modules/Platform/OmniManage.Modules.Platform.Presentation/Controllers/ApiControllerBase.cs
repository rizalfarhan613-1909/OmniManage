using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.BuildingBlocks.Domain;

namespace OmniManage.Modules.Platform.Presentation.Controllers;

public abstract class ApiControllerBase(TimeProvider clock) : ControllerBase
{
    protected IActionResult FromResult<T>(Result<T> result)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsFailure)
        {
            return FromError(result.Error);
        }

        return Ok(ApiResponse<T>.Ok(result.Value, CreateMeta()));
    }

    protected IActionResult FromError(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        var response = ApiResponse<object>.Fail(new ApiError(error.Code, error.Message), CreateMeta());

        return StatusCode(MapStatusCode(error.Type), response);
    }

    private ApiMeta CreateMeta() =>
        new(Activity.Current?.Id ?? HttpContext.TraceIdentifier, clock.GetUtcNow().UtcDateTime);

    private static int MapStatusCode(ErrorType errorType) =>
        errorType switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError,
        };
}
