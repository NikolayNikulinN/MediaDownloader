using MediaDownloader.Tests.Constants;
using Xunit;

namespace MediaDownloader.Tests.Support;

internal static class AsyncAssert
{
    public static async Task WaitUntilAsync(Func<bool> condition, TimeSpan? timeout = null)
    {
        var deadline = DateTime.UtcNow + (timeout ?? TimeSpan.FromSeconds(5));
        while (DateTime.UtcNow < deadline)
        {
            if (condition())
            {
                return;
            }

            await Task.Delay(10);
        }

        Assert.True(condition(), TestConstants.Support.AsyncConditionTimeout);
    }
}
