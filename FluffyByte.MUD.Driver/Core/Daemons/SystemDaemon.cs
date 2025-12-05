/*
 * (SystemDaemon.cs)
 *------------------------------------------------------------
 * Created - 11/23/2025 11:25:01 PM
 * Created by - Seliris
 *-------------------------------------------------------------
 */
using System.Text;
using FluffyByte.MUD.Driver.Core.Types.Daemons;
using FluffyByte.MUD.Driver.FluffyTools;
using FluffyByte.MUD.Driver.Core.Daemons.Networking;
using FluffyByte.MUD.Driver.Core.Daemons.Commands;

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
    private static string Name => "systemd";
    
    #region Life Cycle
    /// <summary>A private <see cref="CancellationTokenSource"/> used to manage the lifecycle of the globally
    /// accessible <see cref="SystemDaemon.GlobalShutdownToken"/>.</summary>
    /// <remarks>The <c>_globalTokenSource</c> is internally used by the <see cref="SystemDaemon"/> class to control
    /// the state of the <c>GlobalShutdownToken</c>. It is reset during the daemon's initialization and
    /// disposed of during shutdown procedures to ensure proper cleanup of resources.
    /// This member should not be accessed directly outside the <c>SystemDaemon</c> implementation.</remarks>
    private static CancellationTokenSource _globalTokenSource;

    /// <summary>A globally accessible token used to signal system-wide shutdown events.</summary>
    /// <remarks>The <c>GlobalShutdownToken</c> is a <see cref="CancellationToken"/> that is triggered when the
    /// system daemon initiates a shutdown process. It allows other daemons and dependent services to observe
    /// and react to a shutdown request, ensuring coordinated termination of operations across the application.
    /// The token is thread-safe and can be used by any component that needs to monitor global shutdown events.
    /// It is instantiated and managed internally by the <see cref="SystemDaemon"/> class, and its lifetime
    /// matches that of the application.</remarks>
    public static CancellationToken GlobalShutdownToken { get; private set; }

    private static DaemonStatus _state = DaemonStatus.Stopped;
    private static DateTime _lastStartTime = DateTime.MaxValue;
    private static TimeSpan Uptime => DateTime.UtcNow - _lastStartTime;
    
    /// <summary>Provides static methods and properties for managing the system daemon, which coordinates global system
    /// tasks and oversees other daemons.</summary>
    /// <remarks>The SystemDaemon class exposes functionality for starting, stopping, and monitoring the status
    /// of the system daemon. All members are static and intended for use in application-wide scenarios where
    /// centralized control of system services is required. Thread safety is ensured for all static members.
    /// The class cannot be instantiated.</remarks>
    static SystemDaemon()
    {
        _globalTokenSource = new CancellationTokenSource();
        GlobalShutdownToken = _globalTokenSource.Token;
    }

    /// <summary>Initializes the system daemon and begins its operation, preparing it to coordinate global system tasks
    /// and oversee other daemons. Ensures the global shutdown token is set up for signaling.</summary>
    /// <returns>A ValueTask representing the asynchronous operation of starting the system daemon.</returns>
    public static async Task RequestStart()
    {
        if (_state is DaemonStatus.Running or DaemonStatus.Starting)
        {
            Log.Warn($"{Name}: RequestStart already in progress or running, current state: {_state}");
            return;
        }

        try
        {
            _state = DaemonStatus.Starting;
            _globalTokenSource = new CancellationTokenSource();
            GlobalShutdownToken = _globalTokenSource.Token;

            Log.Info($"{Name}: Attempting to start all other services...");
            
            // Force daemons awake
            // File must be loaded first, as other Daemons might depend on read/write files.
            FileDaemon.RequestStart();
            NetworkDaemon.RequestStart();
            CommandDaemon.RequestStart();
            await HeartbeatDaemon.RequestStart();
            
            _lastStartTime = DateTime.UtcNow;
            
            Log.Info($"{Name}: All services started successfully.");
            
            _state = DaemonStatus.Running;
        }
        catch (OperationCanceledException)
        {
            // Expected during a shutdown
            Log.Warn($"{Name}: Exception - Operation Canceled during RequestStart of SystemDaemon.");
            _state = DaemonStatus.Error;
        }
        catch (Exception ex)
        {
            Log.Error($"{Name}: Exception in RequestStart()", ex);
            _state = DaemonStatus.Error;
        }
    }

    /// <summary>Initiates a request to gracefully shut down the system daemon, canceling all ongoing operations
    /// and freeing resources.</summary>
    /// <remarks>This method cancels the global shutdown token, disposes of associated resources,
    /// and signals the completion of pending operations. It is designed for use in scenarios where a controlled
    /// shutdown of the system daemon is required. Exceptions that occur during the shutdown process are logged
    /// for diagnostic purposes.</remarks>
    /// <returns>A task that represents the asynchronous shutdown operation.</returns>
    public static async ValueTask RequestShutdown()
    {
        if (_state is DaemonStatus.Stopped or DaemonStatus.Stopping)
        {
            Log.Warn($"{Name}: RequestShutdown already in progress or stopped, current state: {_state}");
            return;
        }
        
        try
        {
            _state = DaemonStatus.Stopping;

            await FileDaemon.ShutdownFlush();
            
            await _globalTokenSource.CancelAsync();
                
            _globalTokenSource.Dispose();
            
            _state = DaemonStatus.Stopped;
        }
        catch (Exception ex)
        {
            Log.Error("CRITICAL ERROR: EXCEPTION DURING SHUTDOWN!", ex);
            _state = DaemonStatus.Error;
        }
    }

    /// <summary>Retrieves the current status of the system daemon, which monitors all other daemons,
    /// providing information about their operational state.</summary>
    /// <remarks>This method evaluates the current operational state of the system daemon and returns an appropriate
    /// status string. It handles exceptions that may occur during execution and provides meaningful
    /// feedback for error or shutdown scenarios.</remarks>
    /// <returns>A string representing the current status of the system daemon. Possible values include
    /// "PROCEEDING TO SHUT DOWN" for a shutdown operation, "ERROR" for unexpected issues, or other context-specific
    /// statuses indicating the system's current state.</returns>
    public static string RequestStatus()
    {
        try
        {
            var output = new StringBuilder();
            
            output.AppendLine("Status of systemd...");
            output.AppendLine($"{Name} -- {_state} -- {Uptime}");
            
            output.AppendLine(NetworkDaemon.RequestStatus);
            output.AppendLine(HeartbeatDaemon.RequestStatus);
            output.AppendLine(FileDaemon.RequestStatus);
            
            return output.ToString();
        }
        catch (OperationCanceledException)
        {
            // Expected during a shutdown
            Log.Warn($"OperationCanceled during RequestStatus of SystemDaemon.");
            return "PROCEEDING TO SHUT DOWN";
        }
        catch (Exception ex)
        {
            Log.Error($"Exception during RequestStatus of SystemDaemon.", ex);
            return "ERROR";
        }
    }
    #endregion Life Cycle
}

/*
*------------------------------------------------------------
* (SystemDaemon.cs)
* See License.txt for licensing information.
*-----------------------------------------------------------
*/