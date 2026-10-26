using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.BuildingBlocks.Contracts.Security;
using OmniManage.Modules.Hris.Application.Payroll.FinalizePayrollRun;
using OmniManage.Modules.Platform.Contracts.Authorization;

namespace OmniManage.Modules.Hris.Presentation.Controllers;

[ApiController]
[Route("api/v1/hris/payroll")]
[RequireModule(ModuleCodes.HrisPayroll)]
public sealed class PayrollController(ISender sender, TimeProvider clock) : ApiControllerBase(clock)
{
    [HttpPost("runs/{id:guid}/finalize")]
    [HasPermission(Permissions.Hris.PayrollApprove)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Finalize(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender
            .Send(new FinalizePayrollRunCommand(id), cancellationToken)
            .ConfigureAwait(false);

        return FromResult(result);
    }
}
