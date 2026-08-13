using MediaDownloader.Services.Updates;
using MediaDownloader.Services.Tests.Constants;
using Xunit;

namespace MediaDownloader.Services.Tests.Updates;

public sealed class ToolVersionComparerTests
{
    [Theory]
    [InlineData(TestConstants.Versions.FfmpegOutput, TestConstants.Versions.FfmpegVersion)]
    [InlineData(TestConstants.Versions.Unexpected, TestConstants.Versions.Unexpected)]
    [InlineData(TestConstants.Versions.UnexpectedPadded, TestConstants.Versions.Unexpected)]
    public void ParseFfmpegVersion_extracts_version_token_or_trimmed_output(string output, string expected)
    {
        Assert.Equal(expected, ToolVersionComparer.ParseFfmpegVersion(output));
    }

    [Theory]
    [InlineData(TestConstants.Versions.CurrentVersion, TestConstants.Versions.LatestVersion, true)]
    [InlineData(TestConstants.Versions.CurrentVersionCustom, TestConstants.Versions.LatestVersion, true)]
    [InlineData(TestConstants.Versions.CurrentVersion, TestConstants.Versions.OlderLatestVersion, false)]
    [InlineData(TestConstants.Versions.UpperCaseVersion, TestConstants.Versions.LowerCaseVersion, true)]
    public void VersionsMatch_accepts_exact_case_insensitive_and_prefixed_builds(
        string currentVersion,
        string latestVersion,
        bool expected)
    {
        Assert.Equal(expected, ToolVersionComparer.VersionsMatch(currentVersion, latestVersion));
    }
}
