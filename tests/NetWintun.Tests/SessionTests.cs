namespace NetWintun.Tests;

public class SessionTests
{

    [Fact]
    public void SimpleReceivePacket()
    {
        using var adapter = Adapter.Create("Demo", "Wintun");
        using var session = adapter.StartSession(Wintun.Constants.MinRingCapacity * 2);
        session.TryReceivePacket(out _);
    }

    [Fact]
    public async Task SimpleReceivePacketAsync()
    {
        using var adapter = Adapter.Create("Demo", "Wintun");
        using var session = adapter.StartSession(Wintun.Constants.MinRingCapacity * 2);
        _ = await session.ReceivePacketAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public void SimpleSendPacket()
    {
        using var adapter = Adapter.Create("Demo", "Wintun");
        using var session = adapter.StartSession(Wintun.Constants.MinRingCapacity * 2);
        session.SendPacket(new byte[5]);
    }

    [Fact]
    public void SendPacketTooBig()
    {
        using var adapter = Adapter.Create("Demo", "Wintun");
        using var session = adapter.StartSession(Wintun.Constants.MinRingCapacity);
        // ReSharper disable once AccessToDisposedClosure
        Assert.Throws<PacketSizeTooLargeException>(() => session.SendPacket(new byte[Wintun.Constants.MaxRingCapacity + 1]));
    }

    [Fact]
    public void SendPacketDispose()
    {
        using var adapter = Adapter.Create("Demo", "Wintun");
        var session = adapter.StartSession(Wintun.Constants.MinRingCapacity);
        session.Dispose();
        Assert.Throws<ObjectDisposedException>(() => session.SendPacket(ReadOnlySpan<byte>.Empty));
    }

    [Fact]
    public async Task ReceivePacketAsyncCancelled()
    {
        using var adapter = Adapter.Create("Demo", "Wintun");
        using var session = adapter.StartSession(Wintun.Constants.MinRingCapacity);
        await Assert.ThrowsAsync<OperationCanceledException>(async () => await session.ReceivePacketAsync(new CancellationToken(true)));
    }

    [Fact]
    public async Task ReceivePacketAsyncDispose()
    {
        using var adapter = Adapter.Create("Demo", "Wintun");
        var session = adapter.StartSession(Wintun.Constants.MinRingCapacity);
        session.Dispose();
        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await session.ReceivePacketAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public void TryReceivePacketDispose()
    {
        using var adapter = Adapter.Create("Demo", "Wintun");
        var session = adapter.StartSession(Wintun.Constants.MinRingCapacity);
        session.Dispose();
        Assert.Throws<ObjectDisposedException>(() => session.TryReceivePacket(out _));
    }
}