using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OmniManage.BuildingBlocks.Application;

public interface IModule
{
    string ModuleCode { get; }

    IReadOnlyCollection<string> HardDependencies { get; }

    void RegisterServices(IServiceCollection services, IConfiguration configuration);
}
