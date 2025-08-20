# NetWintun [![NuGet Version](https://img.shields.io/nuget/v/NetWinTun?logo=nuget)][nuget] [![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)][license]

[**NetWintun**][nuget] is an unofficial **C# wrapper** around the [Wintun][wintun] library.  
Wintun is a minimal, high-performance **TUN driver for Windows**, providing userspace programs with a simple virtual network adapter for sending and receiving packets.

---

## ✨ Features

- 🌀 `async/await` support for receiving packets  
- 📋 Integration with [Microsoft.Extensions.Logging][microsoft.extensions.logging]  
- 🛡️ Robust error handling and resource management  
- 🔌 Ships with the prebuilt, signed [Wintun binaries][prebuilt bins].

---

## 💡 Example Usage

### Sending a packet

```csharp
using var adapter = Adapter.Create("OfficeNet", "Wintun");
using var session = adapter.StartSession(Wintun.Constants.MaxRingCapacity);
session.SendPacket("Hello World"u8);
```

### Receiving a packet

```csharp
using var adapter = Adapter.Create("OfficeNet", "Wintun");
using var session = adapter.StartSession(Wintun.Constants.MinRingCapacity);

// Asynchronously wait for a packet
var packet = await session.ReceivePacketAsync();

// Or try to grab one immediately
if (session.TryReceivePacket(out var packet)) {
    Console.WriteLine($"Received {packet.Length} bytes");
}
```

### Plugging into Microsoft.Extensions.Logging

```csharp
using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
Wintun.SetLogger(loggerFactory.CreateLogger<Program>());
```

👉 Check out the full [example program][example] for a working demo.

---

## 🛠️ Building from Source

### Prerequisites
Before building, unzip the [prebuilt Wintun binaries][prebuilt bins] into the `/wintun` directory:

```bash
/netwintun
 ├─ src/
 ├─ examples/
 └─ wintun/   <-- unzip here
```

---

## 📜 Licensing

The contents of the repository are "Copyright (c) 2025 aneilmac".

Source code: Licensed under the [MIT License][license]

The library requires usage of wintun.dll. The NetWintun nuget package includes prebuilt and signed Wintun binaries, which are distributed under the terms of the Prebuilt Binaries License. By using NetWintun via NuGet, you agree to the terms of the [Wintun Prebuilt Binaries License][prebuilt lic].

---

[wintun]: https://www.wintun.net/ "The Wintun project"
[license]: LICENSE "Project License"
[prebuilt lic]: https://github.com/WireGuard/wintun/blob/master/prebuilt-binaries-license.txt "Wintun Prebuilt Binaries License"
[prebuilt bins]: https://www.wintun.net/builds/wintun-0.14.1.zip "Wintun prebuilt binaries"
[nuget]: https://www.nuget.org/packages/NetWintun "NetWintun on NuGet.org"
[microsoft.extensions.logging]: https://www.nuget.org/packages/Microsoft.Extensions.Logging "Microsoft.Extensions.Logging on NuGet.org"
[example]: examples/Basic/Program.cs "Basic example program"