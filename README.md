# NetWintun

NetWintun is an unofficial C# wrapper around the [Wintun][wintun] library.

Wintun is a very simple and minimal TUN driver for the Windows kernel,
which provides userspace programs with a simple network adapter for reading and writing packets.

## Features

- async/await on receive packets.
- Integration with [Microsoft.Extensions.Logging][microsoft.extensions.logging].
- Robust error handling.

## Example Usage

### Sending a packet

```cs
using var adapter = Adapter.Create("OfficeNet", "Wintun");
using var session = adapter.StartSession(Wintun.Constants.MaxRingCapacity);
session.SendPacket("Hello World"u8);
```

### Receiving a packet

```cs
using var adapter = Adapter.Create("OfficeNet", "Wintun");
using var session = adapter.StartSession(Wintun.Constants.MinRingCapacity);

// This call will asyncronously await for a packet.
var packet = await session.ReceivePacketAsync();

// Instead of waiting, this call will immediately return with a 
// value of `false` if the buffer is empty.
var result = session.TryReceivePacket(out var packet);
```

### Integration with `Microsoft.Extensions.Logging`

```cs
using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
Wintun.SetLogger(loggerFactory.CreateLogger<Program>());
```

An [Example][example] program is provided to demonstrate integration.

## Building 

This project uses the [Nuke][nuke] build system, which can be fetched via:

```sh
dotnet tool install Nuke.GlobalTool --global
```

You may then run the nuke tool from the repo directory.

### Prerequisite

To download and unpack the prebuilt [Wintun][Wintun] librarires run:

```sh
nuke FetchWintun
```

(Alternatively you can manually unzip the contents of [`wintup.zip`][prebuilt bins] to `/wintun`.)

#### Packing nuget package

To create a versioned Wintun package run:

```sh
nuke Pack --configuration Release
```

To cleanup all resources run:

```sh
nuke Clean
```

## Licensing

The entire contents of the repository, including all documentation and example code,
is "Copyright (c) 2025 aneilmac".

Source code is licensed under the [MIT LICENSE][license].

The library requires usage of `wintun.dll`. 
The [NetWintun][nuget] nuget package includes [prebuilt and signed][prebuilt bins] Wintun binaries, which 
are distributed under the terms of the [Prebuilt Binaries License][prebuilt lic]. 
By using the [NetWintun][nuget] nuget package you are also agreeing to the terms of the Prebuilt binaries license.


[wintun]: https://www.wintun.net/ "The Wintun project"
[license]: LICENSE "Project License"
[prebuilt lic]: https://github.com/WireGuard/wintun/blob/master/prebuilt-binaries-license.txt "Wintun Prebuilt Binaries License"
[prebuilt bins]: https://www.wintun.net/builds/wintun-0.14.1.zip "Wintun prebuilt binaries"
[nuget]: https://TODO "NetWintun on Nuget.org"
[microsoft.extensions.logging]: https://www.nuget.org/packages/Microsoft.Extensions.Logging "Microsoft.Extensions.Logging on Nuget.org"
[nuke]: https://nuke.build/ "Nuke CI/CD automation"
[example]: examples/Basic/Program.cs "Basic example program"