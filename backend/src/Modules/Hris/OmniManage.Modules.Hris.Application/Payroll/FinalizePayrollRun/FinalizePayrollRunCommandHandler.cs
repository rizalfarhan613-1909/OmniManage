using Microsoft.Extensions.Logging;
using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Application.IntegrationEvents;
using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Hris.Application.IntegrationEvents;
using OmniManage.Modules.Hris.Contracts.IntegrationEvents;
using OmniManage.Modules.Hris.Domain.Abstractions;
using OmniManage.Modules.Hris.Domain.Errors;

namespace OmniManage.Modules.Hris.Application.Payroll.FinalizePayrollRun;

public sealed class FinalizePayrollRunCommandHandler(
    IPayrollRunRepository payrollRuns,
    IHrisOutbox outbox,
    IIntegrationEventDispatcher dispatcher,
    IGuidGenerator guidGenerator,
    TimeProvider clock,
    ILogger<FinalizePayrollRunCommandHandler> logger)
    : ICommandHandler<FinalizePayrollRunCommand>
{
    public async Task<Result> Handle(
        FinalizePayrollRunCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var payrollRun = await payrollRuns
            .GetForUpdateAsync(command.PayrollRunId, cancellationToken)
            .ConfigureAwait(false);

        if (payrollRun is null)
        {
            return PayrollErrors.NotFound;
        }

        var now = clock.GetUtcNow();

        var finalized = payrollRun.Finalize(now);

        if (finalized.IsFailure)
        {
            return finalized.Error;
        }

        var integrationEvent = new PayrollFinalizedIntegrationEvent(
            guidGenerator.NewGuid(),
            payrollRun.TenantId,
            now.UtcDateTime,
            payrollRun.Id,
            payrollRun.PeriodCode,
            payrollRun.TotalBasicSalary,
            payrollRun.TotalAllowances,
            payrollRun.TotalDeductions,
            TotalKasbonDeducted: 0m,
            TotalTaxPPh21: 0m,
            payrollRun.NetPayTotal);

        // The payroll status and the outbox row are committed together.
        await outbox
            .EnqueueAsync(integrationEvent, cancellationToken)
            .ConfigureAwait(false);
        await payrollRuns
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

        await IntegrationEventDispatch
            .DispatchAfterCommitAsync(outbox, dispatcher, integrationEvent, logger, cancellationToken)
            .ConfigureAwait(false);

        return Result.Success();
    }
}
