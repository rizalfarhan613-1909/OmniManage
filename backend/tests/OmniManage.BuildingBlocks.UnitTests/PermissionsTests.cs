using System.Reflection;
using System.Text.RegularExpressions;
using FluentAssertions;
using OmniManage.BuildingBlocks.Contracts;

namespace OmniManage.BuildingBlocks.UnitTests;

public sealed class PermissionsTests
{
    private static readonly Regex PermissionPattern = new(
        "^[a-z][a-z0-9_]*(\\.[a-z][a-z0-9_]*){2}$",
        RegexOptions.CultureInvariant);

    [Fact]
    public void Constants_WhenEnumerated_AreUniqueAndWellFormed()
    {
        var codes = GetPermissionCodes();

        codes.Should().NotBeEmpty();
        codes.Should().OnlyHaveUniqueItems();
        codes.Should().OnlyContain(code => PermissionPattern.IsMatch(code));
    }

    [Fact]
    public void Constants_WhenEnumerated_ContainDocumentedCodes()
    {
        var codes = GetPermissionCodes();

        codes.Should().Contain(new[]
        {
            "masterdata.business_partner.read",
            "masterdata.business_partner.create",
            "masterdata.item.read",
            "masterdata.item.create",
            "sales.order.read",
            "sales.order.create",
            "sales.order.confirm",
            "finance.invoice.approve",
            "finance.payment.approve",
            "crm.lead.read",
            "crm.customer.read_all",
            "hris.employee.view_sensitive",
            "inventory.warehouse.read",
            "inventory.warehouse.create",
            "inventory.stock.read",
            "inventory.stock.adjust",
        });
    }

    private static string[] GetPermissionCodes() => typeof(Permissions)
        .GetNestedTypes(BindingFlags.Public)
        .SelectMany(type => type.GetFields(BindingFlags.Public | BindingFlags.Static))
        .Where(field => field.IsLiteral && field.FieldType == typeof(string))
        .Select(field => (string)field.GetRawConstantValue()!)
        .ToArray();
}
