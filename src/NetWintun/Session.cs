using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Threading;
using Microsoft.Win32.SafeHandles;

namespace NetWintun;

/// <summary>
/// A Wintun session. Construct via <see cref="Adapter.StartSession"/>.
/// </summary>
public sealed class Session : IDisposable
{
    private readonly SessionHandle _handle;
    private readonly WaitHandle _waitHandle;
    private readonly AsyncManualResetEvent _requestPoke = new();
    private readonly RegisteredWaitHandle _registeredWaitHandle;

    internal Session(SessionHandle handle)
    {
        _handle = handle;
        Exceptions.ThrowWin32If(_handle.IsInvalid);
        var safeWaitHandle = new SafeWaitHandle(PInvoke.GetReadWaitEvent(_handle), false);
        _waitHandle = new SessionWaitHandle { SafeWaitHandle = safeWaitHandle };
        _registeredWaitHandle = ThreadPool.RegisterWaitForSingleObject(
            _waitHandle, (_, _) => _requestPoke.Set(),
            null,
            -1,
            false);
    }

    /// <summary>
    /// Sends the packet. This method is thread safe.
    /// The packet is not guaranteed to be sent yet.
    /// </summary>
    /// <param name="packet">Layer 3 IPv4 or IPv6 packet for sending. Must be less or equal to
    /// <see cref="Wintun.Constants.MaxIpPacketSize"/>.
    /// </param>
    /// <exception cref="PacketSizeTooLargeException">Thrown if packet is too large</exception>
    /// <exception cref="ObjectDisposedException">Thrown if session is disposed.</exception>
    /// <exception cref="System.ComponentModel.Win32Exception">
    /// Thrown when sending fails. (E.g. adapter or buffer is full.)
    /// </exception>
    public void SendPacket(ReadOnlySpan<byte> packet)
    {
        ObjectDisposedException.ThrowIf(_handle.IsClosed, this);
        if (packet.Length > Wintun.Constants.MaxIpPacketSize)
        {
            throw new PacketSizeTooLargeException(
                $"Packet size must not exceed { Wintun.Constants.MaxIpPacketSize}");
        }
        
        unsafe
        {
            var toWrite = PInvoke.AllocateSendPacket(
                _handle, unchecked((uint)packet.Length));
            Exceptions.ThrowWin32If(toWrite == null);
            var outSpan = new Span<byte>(toWrite, packet.Length);
            packet.CopyTo(outSpan);
            PInvoke.SendPacket(_handle, toWrite);
        }
    }

    /// <summary>
    /// Retrieves one or packet. This function is thread-safe.
    /// </summary>
    /// <remarks>
    /// Will wait for packets if none are currently available.
    /// </remarks>
    /// <seealso cref="TryReceivePacket"/>
    /// <param name="cancellationToken">Cancellation token to stop waiting</param>
    /// <returns>Layer 3 IPv4 or IPv6 packet</returns>
    /// <exception cref="ObjectDisposedException">Thrown if session is disposed.</exception>
    /// <exception cref="Win32Exception">
    /// Thrown when receive fails. (E.g. adapter is terminating, buffer is corrupt.)
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown if <paramref name="cancellationToken"/> was cancelled
    /// </exception>
    public async ValueTask<byte[]> ReceivePacketAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_handle.IsClosed, this);
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _requestPoke.Reset();
            if (TryReceivePacket(out var packet))
            {
                return packet;
            }
            await _requestPoke.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Retrieves one or packet. This function is thread-safe.
    /// </summary>
    /// <remarks>
    /// Will return immediately if the buffer is exhausted and there is nothing to retrieve.
    /// </remarks>
    /// <seealso cref="ReceivePacketAsync"/>
    /// <param name="packet">Layer 3 IPv4 or IPv6 packet</param>
    /// <returns><c>true</c> if a packet was received, otherwise <c>false</c> if the buffer was exhausted.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if session is disposed.</exception>
    /// <exception cref="System.ComponentModel.Win32Exception">
    /// Thrown when receive fails. (E.g. adapter is terminating, buffer is corrupt.)
    /// </exception>
    public bool TryReceivePacket(out byte[] packet)
    {
        ObjectDisposedException.ThrowIf(_handle.IsClosed, this);
        unsafe
        {
            var ptr = PInvoke.ReceivePacket(_handle, out var size);
            if (ptr == null)
            {
                var err = Marshal.GetLastWin32Error();
                if (err != Wintun.Constants.ErrorNoMoreItems)
                {
                    throw new Win32Exception(err);
                }
                
                packet = [];
                return false;
            }

            try
            {
                packet = size == 0 ? [] : new Span<byte>(ptr, checked((int)size)).ToArray();
                return true;
            }
            finally
            {
                PInvoke.ReleaseReceivePacket(_handle, ptr);
            }
        }
    }

    public void Dispose()
    {
        _ = _registeredWaitHandle.Unregister(_waitHandle);
        _waitHandle.Dispose();
        _handle.Dispose();
    }

    private class SessionWaitHandle : WaitHandle;
}
