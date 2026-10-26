using System.Reflection;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using OmniManage.Api.Middleware;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.BuildingBlocks.Contracts.Security;
using OmniManage.Modules.AssetMaintenance.Presentation;
using OmniManage.Modules.Finance.Presentation;
using OmniManage.Modules.Hris.Presentation;
using OmniManage.Modules.Manufacturing.Presentation;
using OmniManage.Modules.Platform.Presentation.Controllers;
using OmniManage.Modules.Procurement.Presentation;
using OmniManage.Modules.Projects.Presentation;
using OmniManage.Modules.Sales.Presentation;

namespace OmniManage.Architecture.Tests;

public sealed class ControllerTests
{
    [Fact]
    public void Every_Controller_Should_Have_RequireModule_Or_AllowWithoutModule()
    {
        var missing = Controllers()
            .Where(controller => !DeclaresModuleRequirement(controller))
            .Select(controller => controller.FullName)
            .ToArray();

        missing.Should().BeEmpty(
            "every controller must declare [RequireModule] or [AllowWithoutModule]. Missing: {0}",
            string.Join(", ", missing));
    }

    [Fact]
    public void RequireModule_Should_Use_Known_Module_Code()
    {
        var unknown = Controllers()
            .SelectMany(ModuleRequirements)
            .Select(attribute => attribute.ModuleCode)
            .Where(code => !ModuleCodes.All.Contains(code))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        unknown.Should().BeEmpty(
            "[RequireModule] must use constants from ModuleCodes. Unknown: {0}",
            string.Join(", ", unknown));
    }

    private static Type[] Controllers() =>
        new[]
        {
            typeof(AuthController).Assembly,
            typeof(ModuleLicenseMiddleware).Assembly,
            AssetMaintenancePresentationAssembly.Value,
            FinancePresentationAssembly.Value,
            HrisPresentationAssembly.Value,
            ManufacturingPresentationAssembly.Value,
            ProcurementPresentationAssembly.Value,
            ProjectsPresentationAssembly.Value,
            SalesPresentationAssembly.Value,
        }
            .Distinct()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(IsController)
            .ToArray();

    private static IEnumerable<RequireModuleAttribute> ModuleRequirements(Type controller) =>
        controller
            .GetCustomAttributes<RequireModuleAttribute>(inherit: true)
            .Concat(
                controller
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .SelectMany(method => method.GetCustomAttributes<RequireModuleAttribute>(inherit: true)));

    private static bool IsController(Type type) =>
        type is { IsClass: true, IsAbstract: false } &&
        (typeof(ControllerBase).IsAssignableFrom(type) ||
         type.Name.EndsWith("Controller", StringComparison.Ordinal));

    private static bool DeclaresModuleRequirement(Type controller) =>
        HasModuleAttribute(controller) ||
        controller
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Any(HasModuleAttribute);

    private static bool HasModuleAttribute(MemberInfo member) =>
        member.GetCustomAttributes(inherit: true)
            .Any(attribute => attribute is RequireModuleAttribute or AllowWithoutModuleAttribute);
}
