using System.Reflection;
using System.Text.RegularExpressions;
using FluentAssertions;
using OmniManage.BuildingBlocks.Contracts;

namespace OmniManage.BuildingBlocks.UnitTests;

public sealed class ModuleCodesTests
{
    private static readonly Regex CodePattern = new("^[A-Z][A-Z0-9_]*$", RegexOptions.CultureInvariant);

    [Fact]
    public void Constants_WhenEnumerated_MatchDocumentedCatalogue()
    {
        var codes = GetModuleCodes();

        codes.Should().BeEquivalentTo(new[]
        {
            "PLATFORM_CORE",
            "CORE_MASTERDATA",
            "FIN_GL",
            "FIN_AR",
            "FIN_AP",
            "PROC_CORE",
            "SALES_CORE",
            "INV_CORE",
            "MFG_CORE",
            "HRIS_CORE",
            "HRIS_ATTENDANCE",
            "HRIS_PAYROLL",
            "HRIS_ESS",
            "CRM_CORE",
            "ASSET_CORE",
            "PROJECT_CORE",
        });

        ModuleCodes.All.Should().BeEquivalentTo(codes);
    }

    [Fact]
    public void Constants_WhenEnumerated_AreUniqueAndWellFormed()
    {
        var codes = GetModuleCodes();

        codes.Should().OnlyHaveUniqueItems();
        codes.Should().OnlyContain(code => CodePattern.IsMatch(code));
    }

    private static string[] GetModuleCodes() => typeof(ModuleCodes)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(field => field is { IsLiteral: true } && field.FieldType == typeof(string))
        .Select(field => (string)field.GetRawConstantValue()!)
        .ToArray();
}
