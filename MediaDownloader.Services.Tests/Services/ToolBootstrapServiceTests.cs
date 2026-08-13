using MediaDownloader.Logger.Abstract;
using MediaDownloader.Logger.Enums;
using MediaDownloader.Services.Constants;
using MediaDownloader.Services.Services;
using Xunit;

namespace MediaDownloader.Services.Tests.Services;

public sealed class ToolBootstrapServiceTests
{
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
