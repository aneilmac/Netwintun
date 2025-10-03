using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace NetWintun;

[SupportedOSPlatform("Windows")]
internal class SessionHandle(IntPtr handle) : SafeHandle(handle, true)
{
    public SessionHandle() : this(0)
    {
    }

    protected override bool ReleaseHandle()
    {
        PInvoke.EndSession(handle);
        return true;
    }

    public override bool IsInvalid => handle == 0;
}