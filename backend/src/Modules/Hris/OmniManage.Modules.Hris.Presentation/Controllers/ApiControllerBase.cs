using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Hris.Application.Common;

namespace OmniManage.Modules.Hris.Presentation.Controllers;

public abstract class ApiControllerBase(TimeProvider clock) : ControllerBase
{
    protected IActionResult FromResult<T>(Result<T> result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.IsFailure
            ? FromError(result.Error)
            : Ok(ApiResponse<T>.Ok(result.Value, CreateMeta()));
    }

    protected IActionResult FromResult(Result result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.IsFailure
            ? FromError(result.Error)
            : Ok(ApiResponse<object?>.Ok(null, CreateMeta()));
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

    protected IActionResult FromCreatedResult<T>(Result<T> result, Func<T, string> locationFactory)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(locationFactory);

        if (result.IsFailure)
        {
            return FromError(result.Error);
        }

        Response.Headers.Location = locationFactory(result.Value);

        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<T>.Ok(result.Value, CreateMeta()));
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
