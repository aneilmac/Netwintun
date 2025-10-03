using System.Runtime.Versioning;
using Neovolve.Logging.Xunit;

namespace NetWintun.Tests;

public class LoggerTests : IDisposable
{
    private readonly ICacheLogger _logger;

    [SupportedOSPlatform("Windows")]
    public LoggerTests(ITestOutputHelper testOutput)
    {
        _logger = testOutput.BuildLoggerFor<LoggerTests>();
        Wintun.SetLogger(_logger);
    }

    public void Dispose()
    {
        _logger.Dispose();
    }

    [Fact, SupportedOSPlatform("Windows")]
    public void TestLoggerSetup()
    {
        using var _ = Adapter.Create("Example", "WinTun");
        Assert.True(_logger.Entries.Count > 0);
    }
}