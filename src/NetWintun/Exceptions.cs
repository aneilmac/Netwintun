using System.ComponentModel;
using System.Runtime.InteropServices;

namespace NetWintun;

public class NameTooLongException(string? message) : Exception(message);

public class InvalidCapacityException(string? message) : Exception(message);

public class PacketSizeTooLargeException(string? message) : Exception(message);

internal static class Exceptions
{
    public static void ThrowWin32If(bool condition)
    {
        if (!condition)
        {
            return;
        }

        throw new Win32Exception(Marshal.GetLastWin32Error());
    }
        
    internal static void ThrowIfTooLong(string name)
    {
        if (name.Length > Wintun.Constants.MaxAdapterNameLength)
        {
            throw new NameTooLongException($"{name} exceeded maximum characters of {Wintun.Constants.MaxAdapterNameLength}");
        }
    }
}