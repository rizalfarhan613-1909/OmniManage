using Microsoft.AspNetCore.Mvc;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.BuildingBlocks.Contracts.Security;
using OmniManage.Modules.Platform.Contracts.Authorization;

namespace OmniManage.Api.IntegrationTests.Authorization;

[ApiController]
[Route("api/v1/test/permissions")]
[RequireModule(ModuleCodes.SalesCore)]
public sealed class PermissionTestController : ControllerBase
{
    [HttpGet("granted")]
    [HasPermission(Permissions.Sales.OrderRead)]
    public IActionResult Granted() => Ok(new { authorized = true });

    [HttpGet("denied")]
    [HasPermission(Permissions.Finance.InvoiceApprove)]
    public IActionResult Denied() => Ok(new { authorized = true });
}
