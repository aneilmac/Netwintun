using System.Runtime.Versioning;

namespace NetWintun;

/// <summary>
/// A Wintun adapter.
/// </summary>
public sealed class Adapter : IDisposable
{
    private readonly AdapterHandle _handle;

    /// <summary>
    /// Creates a new Wintun adapter.
    /// </summary>
    /// <param name="name">
    /// The requested name of the adapter. Zero-terminated string of up to
    /// <see cref="Wintun.Constants.MaxAdapterNameLength"/>
    /// characters.</param>
    /// <param name="tunnelType">
    ///  Name of the adapter tunnel type. Zero-terminated string of up to
    /// <see cref="Wintun.Constants.MaxAdapterNameLength"/>
    /// characters.</param>
    /// <param name="requestedGuid">
    /// The GUID of the created network adapter, which then influences NLA generation deterministically.
    /// If it is set to <c>null</c>, the <see cref="Guid"/> is chosen by the system at random,
    /// and hence a new NLA entry is  created for each new adapter.
    /// It is called "requested" <see cref="Guid"/> because the API it uses is
    /// completely undocumented,
    /// and so there could be minor interesting complications with its usage.
    /// </param>
    /// <returns>
    /// If the function succeeds, the return value is the adapter handle. Must be released with
    /// <see cref="Dispose"/>.
    /// </returns>
    /// <exception cref="NameTooLongException">Thrown if <paramref name="name"/> or <paramref name="tunnelType"/> are too long</exception>
    /// <exception cref="System.ComponentModel.Win32Exception">Thrown if construction fails</exception>
    [SupportedOSPlatform("Windows")]
    public static Adapter Create(string name, string tunnelType, Guid? requestedGuid = null)
    {
        Exceptions.ThrowIfTooLong(name);
        Exceptions.ThrowIfTooLong(tunnelType);

        if (requestedGuid == null)
        {
            return new Adapter(PInvoke.CreateAdapter(name, tunnelType, 0));
        }

        var guid = requestedGuid.Value;
        return new Adapter(PInvoke.CreateAdapter(name, tunnelType, guid));
    }

    /// <summary>
    ///  Opens an existing Wintun adapter.
    /// </summary>
    /// <param name="name">
    /// The requested name of the adapter. Zero-terminated string of up to
    /// <see cref="Wintun.Constants.MaxAdapterNameLength"/>
    /// characters.</param>
    /// <returns>
    /// If the function succeeds, the return value is the adapter handle. Must be released with
    /// <see cref="Dispose"/>.
    /// </returns>
    /// <exception cref="NameTooLongException">Thrown if <paramref name="name"/> ise too long</exception>
    /// <exception cref="System.ComponentModel.Win32Exception">Thrown if construction fails</exception>
    [SupportedOSPlatform("Windows")]
    public static Adapter Open(string name)
    {
        Exceptions.ThrowIfTooLong(name);
        return new Adapter(PInvoke.OpenAdapter(name));
    }

    [SupportedOSPlatform("Windows")]
    private Adapter(AdapterHandle handle)
    {
        _handle = handle;
        Exceptions.ThrowWin32If(handle.IsInvalid);
    }

    /// <summary>
    /// Returns the LUID of the adapter.
    /// </summary>
    [SupportedOSPlatform("Windows")]
    public ulong GetLuid()
    {
        ObjectDisposedException.ThrowIf(_handle.IsClosed, this);
        PInvoke.GetAdapterLuid(_handle, out var luid);
        return luid;
    }

    /// <summary>
    /// Starts Wintun session.
    /// </summary>
    /// <param name="capacity">
    /// Rings capacity. Must be between <see cref="Wintun.Constants.MinRingCapacity"/> and
    /// <see cref="Wintun.Constants.MaxRingCapacity"/> (incl.)
    /// Must be a power of two.
    /// </param>
    /// <returns>
    /// Wintun session. Must be released with <see cref="Session.Dispose"/>. 
    /// </returns>
    /// <exception cref="InvalidCapacityException">Thrown if capacity is an invalid value</exception>
    /// <exception cref="ObjectDisposedException">Thrown if this <see cref="Adapter"/> has already been disposed.</exception>
    /// <exception cref="System.ComponentModel.Win32Exception">Thrown if construction fails</exception>
    [SupportedOSPlatform("Windows")]
    public Session StartSession(uint capacity)
    {
        ObjectDisposedException.ThrowIf(_handle.IsClosed, this);

        switch (capacity)
        {
            case < Wintun.Constants.MinRingCapacity:
                throw new InvalidCapacityException($"Capacity must be at least {Wintun.Constants.MinRingCapacity}");
            case > Wintun.Constants.MaxRingCapacity:
                throw new InvalidCapacityException($"Capacity must be at most {Wintun.Constants.MaxRingCapacity}");
        }

        if (!IsPowerOfTwo(capacity))
        {
            throw new InvalidCapacityException("Capacity must be a power of 2");
        }

        return new Session(PInvoke.StartSession(_handle, capacity));

        static bool IsPowerOfTwo(uint x) => (x & (x - 1)) == 0;
    }

    /// <summary>
    /// Releases Wintun adapter resources and, if adapter was created with <see cref="Create"/>, removes adapter.
    /// </summary>
    public void Dispose() => _handle.Dispose();
    
}
