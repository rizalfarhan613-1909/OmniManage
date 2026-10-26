namespace OmniManage.BuildingBlocks.Infrastructure.Export;

internal static class TemporaryFileStream
{
    private const string ExportDirectoryName = "omnimanage-exports";

    /// <summary>
    /// Creates a stream backed by a temporary file that is deleted when the stream
    /// is disposed. This keeps large exports off the managed heap.
    /// </summary>
    public static FileStream Create()
    {
        var directory = Path.Combine(Path.GetTempPath(), ExportDirectoryName);
        Directory.CreateDirectory(directory);

        var path = Path.Combine(directory, $"{Guid.NewGuid():N}.tmp");

        return new FileStream(
            path,
            new FileStreamOptions
            {
                Mode = FileMode.CreateNew,
                Access = FileAccess.ReadWrite,
                Share = FileShare.None,
                BufferSize = 81_920,
                Options = FileOptions.Asynchronous | FileOptions.DeleteOnClose,
            });
    }
}
