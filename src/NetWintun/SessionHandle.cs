using System.Runtime.InteropServices;

namespace NetWintun;

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