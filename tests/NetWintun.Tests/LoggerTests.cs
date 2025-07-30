using Neovolve.Logging.Xunit;

namespace NetWintun.Tests;

public class LoggerTests : IDisposable
{
    private readonly ICacheLogger _logger;

    public LoggerTests(ITestOutputHelper testOutput)
    {
        _logger = testOutput.BuildLoggerFor<LoggerTests>();
        Wintun.SetLogger(_logger);
    }

    public void Dispose()
    {
        _logger.Dispose();
    }

    [Fact]
    public void TestLoggerSetup()
    {
        using var _ = Adapter.Create("Example", "WinTun");
        Assert.True(_logger.Entries.Count > 0);
    }
}