using System.ComponentModel;
using System.Runtime.Versioning;

namespace NetWintun.Tests;


public class DriverTests
{
    [Fact, SupportedOSPlatform("Windows")]
    public void TestDriverDelete()
    {
        using (var _ = Adapter.Create("Demo", "Wintun"))
        {
            Assert.NotEqual(0u, Wintun.GetRunningDriverVersion());
        }
        Wintun.DeleteDriver();
    }
}