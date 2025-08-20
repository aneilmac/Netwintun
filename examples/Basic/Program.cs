/* SPDX-License-Identifier: MIT
 * Copyright (c) 2025 aneilmac
 *
 * A simple test program that waits for and displays incoming packets.
 */

using Microsoft.Extensions.Logging;
using NetWintun;

// Setup the logger
using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
Wintun.SetLogger(loggerFactory.CreateLogger<Program>());

// Create an adapter, which loads up the drivers

using var adapter = Adapter.Create("Basic", "Demo");

Console.WriteLine("Wintun version is: {0}", Wintun.GetRunningDriverVersion());

using var session = adapter.StartSession(Wintun.Constants.MaxRingCapacity);

Console.WriteLine("\nPRESS ANY KEY TO QUIT\n");

var cts = new CancellationTokenSource();
var task = Task.Run(async () =>
{
    // Asynchronously await for incoming packets.
    while (!cts.IsCancellationRequested)
    {
        var packet = await session.ReceivePacketAsync(cts.Token).ConfigureAwait(false);
        var header = packet[..Math.Min(packet.Length, 20)];
        Console.WriteLine("[{0:G}] Packet size: {1:000}. Header: {2}", 
            DateTime.Now, packet.Length, Convert.ToHexString(header));
    }
}, cts.Token);

_ = Console.ReadLine();
try
{
    await Task.WhenAll(cts.CancelAsync(), task).ConfigureAwait(false);
}
catch (OperationCanceledException)
{
    // Suppress cancellation exception
}
