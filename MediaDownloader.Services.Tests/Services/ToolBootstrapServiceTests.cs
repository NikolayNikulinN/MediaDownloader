using MediaDownloader.Logger.Abstract;
using MediaDownloader.Logger.Enums;
using MediaDownloader.Services.Constants;
using MediaDownloader.Services.Services;
using Xunit;

namespace MediaDownloader.Services.Tests.Services;

public sealed class ToolBootstrapServiceTests
{
    [Fact]
    public async Task DownloadMissingToolsAsync_extracts_deno_from_release_archive()
    {
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        using var archiveStream = new MemoryStream();
        using (var archive = new System.IO.Compression.ZipArchive(archiveStream, System.IO.Compression.ZipArchiveMode.Create, true))
        {
            using var entry = archive.CreateEntry(ServiceConstants.Tools.DenoFileName).Open();
            entry.Write(new byte[] { 0x4D, 0x5A, 1, 2 });
        }
        try
        {
            using var httpClient = new HttpClient(new ArchiveHttpMessageHandler(archiveStream.ToArray()));
            var service = new ToolBootstrapService(httpClient, new TestLogger(), directory);
            await service.DownloadMissingToolsAsync([ServiceConstants.Tools.DenoFileName], true,
                cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equal(new byte[] { 0x4D, 0x5A, 1, 2 }, await File.ReadAllBytesAsync(
                Path.Combine(directory, ServiceConstants.Tools.DenoFileName), TestContext.Current.CancellationToken));
            Assert.Single(Directory.GetFiles(directory));
        }
        finally
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
        }
    }

    private sealed class ArchiveHttpMessageHandler(byte[] archive) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new ByteArrayContent(archive) });
    }

    [Fact]
    public async Task DownloadMissingToolsAsync_replaces_tool_in_writable_directory()
    {
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var destination = Path.Combine(directory, ServiceConstants.Tools.YtDlpFileName);
            await File.WriteAllTextAsync(destination, "old version", TestContext.Current.CancellationToken);
            using var httpClient = new HttpClient(new ExecutableHttpMessageHandler());
            var service = new ToolBootstrapService(httpClient, new TestLogger(), directory);

            await service.DownloadMissingToolsAsync([ServiceConstants.Tools.YtDlpFileName], true,
                cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equal(new byte[] { 0x4D, 0x5A, 1, 2 }, await File.ReadAllBytesAsync(destination, TestContext.Current.CancellationToken));
            Assert.False(File.Exists(destination + ServiceConstants.Tools.TemporaryFileExtension));
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public async Task DownloadMissingToolsAsync_http_failure_preserves_installed_tool()
    {
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var destination = Path.Combine(directory, ServiceConstants.Tools.YtDlpFileName);
            await File.WriteAllTextAsync(destination, "old version", TestContext.Current.CancellationToken);
            using var httpClient = new HttpClient(new ExecutableHttpMessageHandler(System.Net.HttpStatusCode.Forbidden));
            var service = new ToolBootstrapService(httpClient, new TestLogger(), directory);

            await Assert.ThrowsAsync<HttpRequestException>(() => service.DownloadMissingToolsAsync(
                [ServiceConstants.Tools.YtDlpFileName], true, cancellationToken: TestContext.Current.CancellationToken));

            Assert.Equal("old version", await File.ReadAllTextAsync(destination, TestContext.Current.CancellationToken));
            Assert.False(File.Exists(destination + ServiceConstants.Tools.TemporaryFileExtension));
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void ToolsDirectoryPath_uses_per_user_storage()
    {
        using var httpClient = new HttpClient(new ThrowingHttpMessageHandler());
        var service = new ToolBootstrapService(httpClient, new TestLogger());
        Assert.Equal(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MediaDownloader", "Tools"), service.ToolsDirectoryPath);
    }

    private sealed class ExecutableHttpMessageHandler(System.Net.HttpStatusCode statusCode = System.Net.HttpStatusCode.OK) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(statusCode) { Content = new ByteArrayContent([0x4D, 0x5A, 1, 2]) });
    }

    [Fact]
    public async Task DownloadMissingToolsAsync_requires_user_confirmation()
    {
        using var httpClient = new HttpClient(new ThrowingHttpMessageHandler());
        var service = new ToolBootstrapService(httpClient, new TestLogger());

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.DownloadMissingToolsAsync(
                [ServiceConstants.Tools.YtDlpFileName],
                userConfirmed: false,
                cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(ServiceConstants.Tools.DownloadRequiresConfirmation, exception.Message);
    }

    [Fact]
    public async Task DownloadMissingToolsAsync_returns_without_http_call_when_missing_tools_is_empty()
    {
        using var httpClient = new HttpClient(new ThrowingHttpMessageHandler());
        var service = new ToolBootstrapService(httpClient, new TestLogger());

        await service.DownloadMissingToolsAsync(
            [],
            userConfirmed: true,
            cancellationToken: TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task DownloadMissingToolsAsync_rejects_null_missing_tools()
    {
        using var httpClient = new HttpClient(new ThrowingHttpMessageHandler());
        var service = new ToolBootstrapService(httpClient, new TestLogger());

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.DownloadMissingToolsAsync(
                null!,
                userConfirmed: true,
                cancellationToken: TestContext.Current.CancellationToken));
    }

    private sealed class ThrowingHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("HTTP should not be called by this test.");
        }
    }

    private sealed class TestLogger : ILoggerWrapper
    {
        public void Write(string message)
        {
        }

        public void Write(string message, LogLevel level)
        {
        }

        public void CloseAndFlush()
        {
        }
    }
}
