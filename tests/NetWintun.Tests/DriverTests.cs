using System.ComponentModel;

namespace NetWintun.Tests;


public class DriverTests
{
    [Fact]
    public void TestDriverDelete()
    {
        using (var _ = Adapter.Create("Demo", "Wintun"))
        {
            Assert.NotEqual(0u, Wintun.GetRunningDriverVersion());
        }
        Wintun.DeleteDriver();
    }
}