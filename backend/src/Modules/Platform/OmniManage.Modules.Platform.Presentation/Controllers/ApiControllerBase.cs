using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Platform.Application.Common;

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

    protected IActionResult FromPagedResult<T>(PagedResult<T> result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var pagination = new PaginationMeta(
            result.Page,
            result.PageSize,
            result.TotalItems,
            result.TotalPages);

        return Ok(ApiResponse<PagedResult<T>>.Ok(result, CreateMeta(pagination)));
    }

    protected IActionResult FromError(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        var response = ApiResponse<object>.Fail(new ApiError(error.Code, error.Message), CreateMeta());

        return StatusCode(MapStatusCode(error.Type), response);
    }

    private ApiMeta CreateMeta(PaginationMeta? pagination = null) =>
        new(Activity.Current?.Id ?? HttpContext.TraceIdentifier, clock.GetUtcNow().UtcDateTime, pagination);

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
