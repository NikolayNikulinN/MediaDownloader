namespace MediaDownloader.Services.Models;

internal sealed record ExtractedToolFile(
                                         string DestinationPath,
                                         string TemporaryPath,
                                         string BackupPath);
