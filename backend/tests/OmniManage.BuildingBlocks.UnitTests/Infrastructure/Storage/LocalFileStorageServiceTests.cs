using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.Extensions.Options;
using OmniManage.BuildingBlocks.Infrastructure;
using OmniManage.BuildingBlocks.Infrastructure.Storage;

namespace OmniManage.BuildingBlocks.UnitTests.Infrastructure.Storage;

public sealed class LocalFileStorageServiceTests : IDisposable
{
    private static readonly Guid TenantId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000701");
    private static readonly Guid OtherTenantId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000702");
    private static readonly DateTimeOffset Now = new(2026, 10, 22, 9, 30, 0, TimeSpan.Zero);

    private readonly string _rootPath = Path.Combine(
        Path.GetTempPath(),
        "omnimanage-storage-tests",
        Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task UploadFileAsync_WhenCalled_StoresFileUnderTenantIsolatedKey()
    {
        var service = CreateService();
        await using var content = new MemoryStream("payslip-content"u8.ToArray());

        var fileKey = await service.UploadFileAsync(
            TenantId,
            "hris_payroll",
            content,
            "payslip.pdf",
            "application/pdf");

        var pattern = $"^{TenantId:D}/HRIS_PAYROLL/2026/10/" +
                      "[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}\\.pdf$";
        Regex.IsMatch(fileKey, pattern).Should().BeTrue("the key convention must be tenant isolated but was {0}", fileKey);

        var path = Path.Combine(_rootPath, fileKey.Replace('/', Path.DirectorySeparatorChar));
        File.Exists(path).Should().BeTrue();
        (await File.ReadAllTextAsync(path)).Should().Be("payslip-content");
    }

    [Fact]
    public async Task GetPreSignedUrlAsync_WhenFileBelongsToAnotherTenant_Throws()
    {
        var service = CreateService();
        await using var content = new MemoryStream("receipt"u8.ToArray());
        var fileKey = await service.UploadFileAsync(
            TenantId,
            "hris_core",
            content,
            "receipt.png",
            "image/png");

        var action = () => service.GetPreSignedUrlAsync(OtherTenantId, fileKey, TimeSpan.FromMinutes(15));

        await action.Should().ThrowAsync<StorageTenantIsolationException>();
    }

    [Fact]
    public async Task GetPreSignedUrlAsync_WhenFileBelongsToTenant_ReturnsUri()
    {
        var service = CreateService();
        await using var content = new MemoryStream("receipt"u8.ToArray());
        var fileKey = await service.UploadFileAsync(
            TenantId,
            "hris_core",
            content,
            "receipt.png",
            "image/png");

        var url = await service.GetPreSignedUrlAsync(TenantId, fileKey, TimeSpan.FromMinutes(15));

        var uri = new Uri(url);
        uri.IsFile.Should().BeTrue();
        File.Exists(uri.LocalPath).Should().BeTrue();
    }

    [Fact]
    public async Task DeleteFileAsync_WhenFileBelongsToTenant_RemovesFile()
    {
        var service = CreateService();
        await using var content = new MemoryStream("asset-label"u8.ToArray());
        var fileKey = await service.UploadFileAsync(
            TenantId,
            "asset_core",
            content,
            "label.pdf",
            "application/pdf");
        var path = Path.Combine(_rootPath, fileKey.Replace('/', Path.DirectorySeparatorChar));

        await service.DeleteFileAsync(TenantId, fileKey);

        File.Exists(path).Should().BeFalse();
    }

    [Fact]
    public async Task UploadFileAsync_WhenModuleCodeContainsPathSeparator_Throws()
    {
        var service = CreateService();
        await using var content = new MemoryStream("payload"u8.ToArray());

        var action = () => service.UploadFileAsync(
            TenantId,
            "../escape",
            content,
            "file.txt",
            "text/plain");

        await action.Should().ThrowAsync<ArgumentException>();
    }

    public void Dispose()
    {
        if (Directory.Exists(_rootPath))
        {
            Directory.Delete(_rootPath, recursive: true);
        }
    }

    private LocalFileStorageService CreateService() =>
        new(
            Options.Create(new StorageOptions
            {
                Provider = StorageProvider.Local,
                LocalRootPath = _rootPath,
            }),
            new FixedTimeProvider(Now),
            new GuidGenerator());

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
