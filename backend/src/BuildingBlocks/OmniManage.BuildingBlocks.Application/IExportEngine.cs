namespace OmniManage.BuildingBlocks.Application;

/// <summary>
/// Universal document generation engine used by modules to produce
/// standardized PDF and Excel exports. Implementations must stream output
/// so large exports stay below the configured memory budget.
/// </summary>
public interface IExportEngine
{
    /// <summary>
    /// Renders HTML content to a PDF document.
    /// </summary>
    /// <returns>A readable stream containing the generated PDF.</returns>
    Task<Stream> GeneratePdfAsync(string htmlContent, CancellationToken ct = default);

    /// <summary>
    /// Generates a single-sheet Excel workbook from a set of rows.
    /// </summary>
    /// <returns>A readable stream containing the generated workbook.</returns>
    Task<Stream> GenerateExcelAsync<T>(
        IReadOnlyCollection<T> rows,
        string worksheetName,
        CancellationToken ct = default);
}
