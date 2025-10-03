using System.Runtime.Versioning;
using Microsoft.Extensions.Logging;

namespace NetWintun;

/// <summary>
/// Wintun static methods. Use this to retrieve driver information or setup logging.
/// </summary>
public static class Wintun
{
    public static class Constants
    {
        internal const int ErrorNoMoreItems = 0x103;

        /// <summary>
        /// Maximum string length for adapter names.
        /// </summary>
        public const int MaxAdapterNameLength = 0x7F;

        /// <summary>
        /// Minimum ring capacity.
        /// </summary>
        public const int MinRingCapacity = 0x20000; /* 128kiB */

        /// <summary>
        /// Maximum ring capacity.
        /// </summary>
        public const int MaxRingCapacity = 0x4000000; /* 64MiB */

        /// <summary>
        /// Maximum IP packet size
        /// </summary>
        public const int MaxIpPacketSize = 0xFFFF;
    }

    /// <summary>
    /// Deletes the Wintun driver if there are no more adapters in use.
    /// </summary>
    /// <exception cref="System.ComponentModel.Win32Exception">
    /// Thrown if function fails
    /// </exception>
    [SupportedOSPlatform("Windows")]
    public static void DeleteDriver()
    {
        Exceptions.ThrowWin32If(!PInvoke.DeleteDriver());
    }

    /// <summary>
    /// Determines the version of the Wintun driver currently loaded.
    /// </summary>
    /// <returns>Version number</returns>
    /// <exception cref="System.ComponentModel.Win32Exception">
    /// Thrown if version retrieval fails
    /// </exception>
    [SupportedOSPlatform("Windows")]
    public static uint GetRunningDriverVersion()
    {
        var version = PInvoke.GetRunningDriverVersion();
        Exceptions.ThrowWin32If(version == 0);
        return version;
    }

    private static PInvoke.LogCallback? _callback;

    /// <summary>
    /// Sets the logger.
    /// </summary>
    /// <param name="logger">Logger to log to, set to <c>null</c> to disable.</param>
    [SupportedOSPlatform("Windows")]
    public static void SetLogger(ILogger? logger)
    {
        _callback = Callback; // GC protection
        PInvoke.SetLogger(logger == null ? null : _callback);
        return;

        void Callback(
            LoggingLevel level, 
            ulong timestamp,
            string message
        )
        {
            try
            {
                logger!.Log(level switch
                    {
                        LoggingLevel.LogInfo => LogLevel.Information,
                        LoggingLevel.LogWarn => LogLevel.Warning,
                        LoggingLevel.LogErr => LogLevel.Error,
                        _ => LogLevel.Critical
                    },
                    "[{DateTime:u}] {Message} ",
                    DateTime.FromFileTime(unchecked((long)timestamp)),
                    message);
            }
            catch
            {
                // Should not throw in callbacks, this would crash application.
            }
        }
    }
}
