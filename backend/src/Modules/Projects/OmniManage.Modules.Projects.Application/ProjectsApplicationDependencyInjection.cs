using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OmniManage.BuildingBlocks.Application.Validation;
using OmniManage.Modules.Projects.Application.Tasks.CreateProjectTask;
using OmniManage.Modules.Projects.Application.WorkLogs.RecordWorkLog;

namespace OmniManage.Modules.Projects.Application;

public static class ProjectsApplicationDependencyInjection
{
    public static IServiceCollection AddProjectsApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(typeof(ProjectsApplicationDependencyInjection).Assembly);
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddScoped<IValidator<CreateProjectTaskCommand>, CreateProjectTaskValidator>();
        services.AddScoped<IValidator<RecordWorkLogCommand>, RecordWorkLogValidator>();

        return services;
    }
}
