using Microsoft.AspNetCore.Mvc;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.BuildingBlocks.Contracts.Security;

namespace OmniManage.Api.IntegrationTests.Licensing;

[ApiController]
[Route("api/v1/test/licensing")]
[RequireModule(ModuleCodes.SalesCore)]
public sealed class ModuleLicenseTestController : ControllerBase
{
    [HttpGet]
    public IActionResult GetLicensedResource() => Ok(new { licensed = true });

    [HttpGet("open")]
    [AllowWithoutModule]
    public IActionResult GetOpenResource() => Ok(new { licensed = false });
}
