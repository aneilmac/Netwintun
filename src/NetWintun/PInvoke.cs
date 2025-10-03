using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace NetWintun;


[SupportedOSPlatform("Windows")]
internal static partial class PInvoke
{
    [LibraryImport("wintun", EntryPoint = "WintunCreateAdapter", SetLastError = true,
        StringMarshalling = StringMarshalling.Utf16)]
    internal static partial AdapterHandle CreateAdapter(string name, string tunnelType, in Guid requestedGuid);

    [LibraryImport("wintun", EntryPoint = "WintunCreateAdapter", SetLastError = true,
        StringMarshalling = StringMarshalling.Utf16)]
    internal static partial AdapterHandle CreateAdapter(string name, string tunnelType, IntPtr @null);

    [LibraryImport("wintun", EntryPoint = "WintunOpenAdapter", SetLastError = true,
        StringMarshalling = StringMarshalling.Utf16)]
    internal static partial AdapterHandle OpenAdapter(string name);

    [LibraryImport("wintun", EntryPoint = "WintunCloseAdapter")]
    internal static partial void CloseAdapter(IntPtr adapter);

    [LibraryImport("wintun", EntryPoint = "WintunDeleteDriver", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool DeleteDriver();

    [LibraryImport("wintun", EntryPoint = "WintunGetAdapterLUID")]
    internal static partial void GetAdapterLuid(AdapterHandle adapter, out ulong luId);

    [LibraryImport("wintun", EntryPoint = "WintunGetRunningDriverVersion")]
    internal static unsafe partial uint GetRunningDriverVersion();

    internal delegate void LogCallback(
        [MarshalAs(UnmanagedType.I4)] LoggingLevel level, 
        ulong timestamp,
        [MarshalAs(UnmanagedType.LPWStr)] string message
    );

    [LibraryImport("wintun", EntryPoint = "WintunSetLogger")]
    internal static partial void SetLogger([MarshalAs(UnmanagedType.FunctionPtr)] LogCallback? newLog);

    [LibraryImport("wintun", EntryPoint = "WintunStartSession", SetLastError = true)]
    internal static partial SessionHandle StartSession(AdapterHandle adapter, uint capacity);

    [LibraryImport("wintun", EntryPoint = "WintunEndSession")]
    internal static partial void EndSession(IntPtr session);

    [LibraryImport("wintun", EntryPoint = "WintunGetReadWaitEvent")]
    internal static partial IntPtr GetReadWaitEvent(SessionHandle session);

    [LibraryImport("wintun", EntryPoint = "WintunReceivePacket", SetLastError = true)]
    internal static unsafe partial byte* ReceivePacket(SessionHandle session, out uint packetSize);

    [LibraryImport("wintun", EntryPoint = "WintunReleaseReceivePacket")]
    internal static unsafe partial void ReleaseReceivePacket(SessionHandle session, byte* packet);

    [LibraryImport("wintun", EntryPoint = "WintunAllocateSendPacket", SetLastError = true)]
    internal static unsafe partial byte* AllocateSendPacket(SessionHandle session, uint packetSize);

    [LibraryImport("wintun", EntryPoint = "WintunSendPacket")]
    internal static unsafe partial void SendPacket(SessionHandle session, byte* packet);
}

