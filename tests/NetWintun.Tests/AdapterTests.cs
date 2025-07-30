using System.ComponentModel;
using System.Text;

namespace NetWintun.Tests;

public class AdapterTests
{
    [Fact]
    public void TestSimpleConstruction()
    {
        using var adapter1 = Adapter.Create("Demo", "Wintun");
    }

    [Fact]
    public void TestConstructionWithGuid()
    {
        using var adapter1 = Adapter.Create("Demo", "Wintun", Guid.NewGuid());
    }

    [Fact]
    public void TestCreateNameTooLong()
    {
        var chars = new byte[Wintun.Constants.MaxAdapterNameLength + 1];
        chars.AsSpan().Fill(0x5A); // Fill with letter 'Z';
        Assert.Throws<NameTooLongException>(() => Adapter.Create(Encoding.ASCII.GetString(chars), "Wintun"));
    }

    [Fact]
    public void TestCreateTunnelTypeTooLong()
    {
        var chars = new byte[Wintun.Constants.MaxAdapterNameLength + 1];
        chars.AsSpan().Fill(0x5A); // Fill with letter 'Z';
        Assert.Throws<NameTooLongException>(() => Adapter.Create("Demo", Encoding.ASCII.GetString(chars)));
    }

    [Fact]
    public void TestOpenNameTooLong()
    {
        var chars = new byte[Wintun.Constants.MaxAdapterNameLength + 1];
        chars.AsSpan().Fill(0x5A); // Fill with letter 'Z';
        Assert.Throws<NameTooLongException>(() => Adapter.Open(Encoding.ASCII.GetString(chars)));
    }

    [Fact]
    public void TestOpenThrowsNotFound()
    {
        Assert.Throws<Win32Exception>(() => Adapter.Open("ServiceThatDoesNotExist"));
    }

    [Fact]
    public void TestGetLuid()
    {
        using var adapter1 = Adapter.Create("Demo", "Wintun");
        Assert.NotEqual(0u, adapter1.GetLuid());
    }

    [Fact]
    public void TestGetLuidDisposed()
    {
        var adapter1 = Adapter.Create("Demo", "Wintun");
        adapter1.Dispose();
        Assert.Throws<ObjectDisposedException>(() => adapter1.GetLuid());
    }

    [Fact]
    public void TestStartSessionDisposed()
    {
        var adapter1 = Adapter.Create("Demo", "Wintun");
        adapter1.Dispose();
        Assert.Throws<ObjectDisposedException>(() => adapter1.StartSession(Wintun.Constants.MinRingCapacity));
    }

    [Fact]
    public void TestRingBufferTooSmall()
    {
        using var adapter1 = Adapter.Create("Demo", "Wintun");
        // ReSharper disable once AccessToDisposedClosure
        Assert.Throws<InvalidCapacityException>(() => adapter1.StartSession(Wintun.Constants.MinRingCapacity / 2));
    }

    [Fact]
    public void TestRingBufferTooBig()
    {
        using var adapter1 = Adapter.Create("Demo", "Wintun");
        // ReSharper disable once AccessToDisposedClosure
        Assert.Throws<InvalidCapacityException>(() => adapter1.StartSession(Wintun.Constants.MaxRingCapacity * 2));
    }

    [Fact]
    public void TestRingBufferNotPower2()
    {
        using var adapter1 = Adapter.Create("Demo", "Wintun");
        // ReSharper disable once AccessToDisposedClosure
        Assert.Throws<InvalidCapacityException>(() => adapter1.StartSession(Wintun.Constants.MinRingCapacity + 1));
    }
}