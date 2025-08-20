using System.Reflection;
using NetWintun.Tests;
using System.Runtime.InteropServices;

[assembly: AssemblyFixture(typeof(LoadLibraryFixture))]
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace NetWintun.Tests;

public class LoadLibraryFixture
{
    public LoadLibraryFixture()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            throw new PlatformNotSupportedException();
        }

        NativeLibrary.TryLoad(Path.Join(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "runtimes", RuntimeInformation.RuntimeIdentifier, "native", "wintun.dll"),
            out _);
    }
}