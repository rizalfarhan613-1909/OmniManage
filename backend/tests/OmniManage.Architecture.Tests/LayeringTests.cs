using System.Reflection;
using FluentAssertions;
using NetArchTest.Rules;
using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.BuildingBlocks.Domain;
using OmniManage.BuildingBlocks.Infrastructure.Persistence;

namespace OmniManage.Architecture.Tests;

public sealed class LayeringTests
{
    [Fact]
    public void Domain_Should_Not_Depend_On_Outer_Layers()
    {
        AssertNoDependencies(
            typeof(Entity<>).Assembly,
            "OmniManage.BuildingBlocks.Application",
            "OmniManage.BuildingBlocks.Infrastructure",
            "OmniManage.BuildingBlocks.Contracts",
            "Microsoft.EntityFrameworkCore",
            "Microsoft.AspNetCore");
    }

    [Fact]
    public void Application_Should_Not_Depend_On_Infrastructure_Or_Host()
    {
        AssertNoDependencies(
            typeof(ICommand).Assembly,
            "OmniManage.BuildingBlocks.Infrastructure",
            "OmniManage.Api",
            "Microsoft.EntityFrameworkCore",
            "Microsoft.AspNetCore");
    }

    [Fact]
    public void Contracts_Should_Not_Depend_On_Other_BuildingBlocks()
    {
        AssertNoDependencies(
            typeof(ModuleCodes).Assembly,
            "OmniManage.BuildingBlocks.Domain",
            "OmniManage.BuildingBlocks.Application",
            "OmniManage.BuildingBlocks.Infrastructure",
            "Microsoft.EntityFrameworkCore",
            "Microsoft.AspNetCore");
    }

    [Fact]
    public void Infrastructure_Should_Not_Depend_On_Host_Or_Modules()
    {
        AssertNoDependencies(
            typeof(BaseDbContext).Assembly,
            "OmniManage.Api",
            "OmniManage.Modules");
    }

    [Fact]
    public void BuildingBlocks_Should_Not_Depend_On_Module_Internals()
    {
        var assemblies = new[]
        {
            typeof(Entity<>).Assembly,
            typeof(ICommand).Assembly,
            typeof(ModuleCodes).Assembly,
            typeof(BaseDbContext).Assembly,
        };

        var result = Types.InAssemblies(assemblies)
            .ShouldNot()
            .HaveDependencyOn("OmniManage.Modules")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "building blocks must never depend on module internals. Violations: {0}",
            DescribeFailures(result));
    }

    private static void AssertNoDependencies(Assembly assembly, params string[] forbiddenDependencies)
    {
        var result = Types.InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAny(forbiddenDependencies)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "{0} must not depend on {1}. Violations: {2}",
            assembly.GetName().Name,
            string.Join(", ", forbiddenDependencies),
            DescribeFailures(result));
    }

    private static string DescribeFailures(TestResult result) =>
        result.FailingTypeNames is null ? "(none)" : string.Join(", ", result.FailingTypeNames);
}
