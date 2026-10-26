using System.IO.Compression;
using System.Text;
using FluentAssertions;
using OmniManage.BuildingBlocks.Infrastructure.Export;

namespace OmniManage.BuildingBlocks.UnitTests.Infrastructure.Export;

public sealed class UniversalExportEngineTests
{
    [Fact]
    public async Task GeneratePdfAsync_WhenGivenHtml_ReturnsPdfDocument()
    {
        var engine = new UniversalExportEngine();
        const string html = """
            <h1>Payslip</h1>
            <p>Employee: Ada Lovelace</p>
            <table>
              <tr><th>Component</th><th>Amount</th></tr>
              <tr><td>Basic salary</td><td>10.000.000</td></tr>
            </table>
            """;

        await using var stream = await engine.GeneratePdfAsync(html);

        stream.Length.Should().BeGreaterThan(500);

        var header = new byte[5];
        stream.Position = 0;
        var bytesRead = await stream.ReadAsync(header);

        bytesRead.Should().Be(5);
        Encoding.ASCII.GetString(header).Should().Be("%PDF-");
    }

    [Fact]
    public async Task GeneratePdfAsync_WhenHtmlIsEmpty_Throws()
    {
        var engine = new UniversalExportEngine();

        var action = () => engine.GeneratePdfAsync("  ");

        await action.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GenerateExcelAsync_WhenGivenRows_ReturnsOpenXmlWorkbook()
    {
        var engine = new UniversalExportEngine();
        var rows = new[]
        {
            new PayslipRow("Ada Lovelace", 10_000_000m, new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc)),
            new PayslipRow("Grace Hopper", 12_500_000m, new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc)),
        };

        await using var stream = await engine.GenerateExcelAsync(rows, "Payslips");

        stream.Length.Should().BeGreaterThan(500);

        using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true);

        archive.GetEntry("xl/workbook.xml").Should().NotBeNull();
        archive.GetEntry("xl/worksheets/sheet1.xml").Should().NotBeNull();
    }

    [Fact]
    public async Task GenerateExcelAsync_WhenWorksheetNameIsInvalid_NormalizesSheetName()
    {
        var engine = new UniversalExportEngine();
        var rows = new[] { new PayslipRow("Ada Lovelace", 10_000_000m, DateTime.UtcNow) };

        await using var stream = await engine.GenerateExcelAsync(rows, "Payroll/October:2026");

        using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true);
        var workbook = archive.GetEntry("xl/workbook.xml");

        workbook.Should().NotBeNull();

        using var reader = new StreamReader(workbook!.Open());
        var content = await reader.ReadToEndAsync();

        content.Should().Contain("name=\"Payroll October 2026\"");
    }

    private sealed record PayslipRow(string EmployeeName, decimal NetPay, DateTime PaidOn);
}
