using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OmniManage.BuildingBlocks.Application.Validation;
using OmniManage.Modules.Hris.Application.Attendance.ClockIn;
using OmniManage.Modules.Hris.Application.Payroll.FinalizePayrollRun;

namespace OmniManage.Modules.Hris.Application;

public static class HrisApplicationDependencyInjection
{
    public static IServiceCollection AddHrisApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(typeof(HrisApplicationDependencyInjection).Assembly);
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddScoped<IValidator<ClockInCommand>, ClockInValidator>();
        services.AddScoped<IValidator<FinalizePayrollRunCommand>, FinalizePayrollRunValidator>();

        return services;
    }
}
