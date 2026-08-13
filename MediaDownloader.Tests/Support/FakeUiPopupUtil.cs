using MediaDownloader.Presentation.Abstract;
using MediaDownloader.Presentation.Models;

namespace MediaDownloader.Tests.Support;

internal sealed class FakeUiPopupUtil : IUiPopupUtil
{
    public bool ConfirmStopDownloadResult { get; set; } = true;

    public bool ConfirmAllMediaItemsDownloadResult { get; set; } = true;

    public bool ConfirmToolUpdateResult { get; set; } = true;

    public DeleteMediaItemConfirmationResult ConfirmMediaItemDeleteResult { get; set; } = new() { IsConfirmed = true };

    public DeleteMediaItemConfirmationResult ConfirmAllMediaItemsDeleteResult { get; set; } = new() { IsConfirmed = true };

    public List<string> Errors { get; } = [];

    public List<string> Warnings { get; } = [];

    public List<string> Messages { get; } = [];

    public List<IReadOnlyCollection<string>> ToolUpdateConfirmations { get; } = [];

    public int ToolUpdateProgressCalls { get; private set; }

    public void ShowMessage(string message) => Messages.Add(message);

    public void ShowWarning(string message) => Warnings.Add(message);

    public void ShowError(string message) => Errors.Add(message);

    public bool ConfirmToolDownload(IReadOnlyCollection<string> missingTools) => true;

    public bool ConfirmToolUpdate(IReadOnlyCollection<string> updates)
    {
        ToolUpdateConfirmations.Add(updates);
        return ConfirmToolUpdateResult;
    }

    public bool ConfirmStopDownload() => ConfirmStopDownloadResult;

    public DeleteMediaItemConfirmationResult ConfirmMediaItemDelete(bool canDeleteDownloadedFile) => ConfirmMediaItemDeleteResult;

    public DeleteMediaItemConfirmationResult ConfirmAllMediaItemsDelete(bool canDeleteDownloadedFile) => ConfirmAllMediaItemsDeleteResult;

    public bool ConfirmAllMediaItemsDownload() => ConfirmAllMediaItemsDownloadResult;

    public Task ShowToolDownloadProgressAsync(Func<IProgress<string>, Task> downloadOperation) =>
        downloadOperation(new Progress<string>());

    public async Task ShowToolUpdateProgressAsync(Func<IProgress<string>, Task> updateOperation)
    {
        ToolUpdateProgressCalls++;
        await updateOperation(new Progress<string>());
    }
}
