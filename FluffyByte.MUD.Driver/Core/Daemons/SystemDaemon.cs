/*
 * (SystemDaemon.cs)
 *------------------------------------------------------------
 * Created - 11/23/2025 11:25:01 PM
 * Created by - Seliris
 *-------------------------------------------------------------
 */
using System.Text;
using FluffyByte.MUD.Driver.Core.Daemons.NetworkD;
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
    public static CancellationToken GlobalShutdownToken => _globalCts.Token;

    private static CancellationTokenSource _globalCts = new();
    /// <summary>
    /// The name of the daemon.
    /// </summary>
    public static string Name => "systemd";

    private static DateTime _lastStartTime = DateTime.UtcNow;
    
    /// <summary>
    /// The present up time (as a TimeSpan) of the daemon.
    /// </summary>
    public static TimeSpan Uptime => DateTime.UtcNow - _lastStartTime;
    
    /// <summary>
    /// Current status of the Daemon
    /// 1. Stopped
    /// 2. Starting
    /// 3. Running
    /// 4. Stopping
    /// 5. Error
    /// </summary>
    public static DaemonStatus State { get; private set; }

    /// <summary>
    /// Starts the System Daemon
    /// The System Daemon is responsible for managing global system tasks and coordinating other daemons.
    /// </summary>
    /// <returns></returns>
    public static async ValueTask RequestStart()
    {
        if (State == DaemonStatus.Starting || State == DaemonStatus.Running)
        {
            Log.Error(
                $"Cannot start {Name} because it is already running or in the process of starting. Current State: {State}"
            );
            return;
        }

        State = DaemonStatus.Starting;

        try
        {
            if (_globalCts.IsCancellationRequested)
            {
                _globalCts.Dispose();
                _globalCts = new CancellationTokenSource();
            }

            Log.Debug($"Requesting start of FileDaemon and NetworkDaemon...");
            
            await FileDaemon.RequestStart();
            
            await NetworkDaemon.RequestStart();
            
            Log.Debug($"FileDaemon and NetworkDaemon started.");
        }
        catch (OperationCanceledException)
        {
            Log.Debug($"Operation canceled, this is expected if it's a shutdown.");
        }
        catch (Exception ex)
        {
            Log.Error($"Exception in RequestStart()", ex);

            State = DaemonStatus.Error;
        }
        finally
        {
            _lastStartTime = DateTime.UtcNow;
            State = DaemonStatus.Running;
        }
    }

    /// <summary>
    /// Stops the System Daemon
    /// The System Daemon is responsible for managing global system tasks and coordinating other daemons.
    /// </summary>
    public static async ValueTask RequestStop()
    {
        Log.Debug($"Shutdown called on {Name}.");

        State = DaemonStatus.Stopping;

        try
        {
            await _globalCts.CancelAsync();
        }
        catch (OperationCanceledException)
        {
            Log.Debug($"Operation canceled during shutdown.");
        }
        catch (Exception ex)
        {
            Log.Error($"Execption in RequestStop()", ex);

            State = DaemonStatus.Error;
        }
        finally
        {
            State = DaemonStatus.Stopped;
        }
    }

    /// <summary>
    /// Retrieves a formatted status summary of the current file daemon, including its name, state, and uptime.
    /// </summary>
    /// <remarks>The returned string provides a concise overview of the file daemon's operational status.
    /// Future enhancements may include additional daemons in the summary output.</remarks>
    /// <returns>A string containing the name, state, and uptime of the file daemon, separated by delimiters.</returns>
    public static string RequestStatus()
    {
        StringBuilder sb = new();

        sb.AppendLine($"{NetworkDaemon.Name} -- {NetworkDaemon.State} -- {NetworkDaemon.Uptime}");
        
        sb.AppendLine($"{FileDaemon.Name} -- {FileDaemon.State} -- {FileDaemon.Uptime}");
        // Add future daemons here

        return sb.ToString();
    }
}

/*
*------------------------------------------------------------
* (SystemDaemon.cs)
* See License.txt for licensing information.
*-----------------------------------------------------------
*/