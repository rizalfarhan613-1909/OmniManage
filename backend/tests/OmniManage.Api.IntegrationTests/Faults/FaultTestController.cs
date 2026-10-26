using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OmniManage.Api.IntegrationTests.Faults;

/// <summary>
/// Test-only endpoint used to verify that unexpected exceptions are converted
/// to the standard envelope by <c>GlobalExceptionMiddleware</c>.
/// </summary>
[ApiController]
[Route("__test/fault")]
[AllowAnonymous]
public sealed class FaultTestController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() =>
        throw new InvalidOperationException("Intentional integration-test failure.");
}
