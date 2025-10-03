using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace NetWintun;

[SupportedOSPlatform("Windows")]
internal class AdapterHandle(IntPtr handle) : SafeHandle(handle, true)
{
    public AdapterHandle() : this(0)
    {
    }

    protected override bool ReleaseHandle()
    {
        PInvoke.CloseAdapter(handle);
        return true;
    }

    public override bool IsInvalid => handle == 0;
}

