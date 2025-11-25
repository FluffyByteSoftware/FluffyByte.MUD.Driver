/*
 * (SystemDaemon.cs)
 *------------------------------------------------------------
 * Created - 11/23/2025 11:25:01 PM
 * Created by - Seliris
 *-------------------------------------------------------------
 */

using System.Runtime.CompilerServices;
using FluffyByte.MUD.Driver.Core.Daemons.FileManager;
using FluffyByte.MUD.Driver.Core.Types.Daemons;
using FluffyByte.MUD.Driver.FluffyTools;

namespace FluffyByte.MUD.Driver.Core.Daemons;

/// <summary>
/// Provides static methods and properties for managing the system daemon, which coordinates global system tasks and
/// oversees other daemons.
/// </summary>
/// <remarks>The SystemDaemon class exposes functionality for starting, stopping, and monitoring the status of the
/// system daemon. All members are static and intended for use in application-wide scenarios where centralized control
/// of system services is required. Thread safety is ensured for all static members. The class is not intended to be
/// instantiated.</remarks>
public static class SystemDaemon
{
    /// <summary>
    /// The Global CancellationToken used to signal shutdown across the system.
    /// </summary>
    public static CancellationToken GlobalShutdownToken { get; private set; }

    /// <summary>
    /// The name of the daemon.
    /// </summary>
    public static string Name => "systemd";

    /// <summary>
    /// Current status of the Daemon
    /// 1. Stopped
    /// 2. Starting
    /// 3. Running
    /// 4. Stopping
    /// 5. Error
    /// </summary>
    public static DaemonStatus Status { get; private set; }

    /// <summary>
    /// Starts the System Daemon
    /// The System Daemon is responsible for managing global system tasks and coordinating other daemons.
    /// </summary>
    /// <returns></returns>
    public static async ValueTask RequestStart()
    {
        Log.Debug($"Initialization called on {Name}.");
        
        GlobalShutdownToken = new();

        await FileDaemon.RequestStart();
        Log.Debug($"GlobalShutdownToken registered.");
    }

    /// <summary>
    /// Stops the System Daemon
    /// The System Daemon is responsible for managing global system tasks and coordinating other daemons.
    /// </summary>
    public static async ValueTask RequestStop()
    {
        Log.Debug($"Shutdown called on {Name}.");

        CancellationTokenSource _cts = CancellationTokenSource.CreateLinkedTokenSource(GlobalShutdownToken);

        await _cts.CancelAsync();

        await FileDaemon.RequestStop();

        Log.Debug($"GlobalShutdownToken cancelled.");
    }
}

/*
*------------------------------------------------------------
* (SystemDaemon.cs)
* See License.txt for licensing information.
*-----------------------------------------------------------
*/