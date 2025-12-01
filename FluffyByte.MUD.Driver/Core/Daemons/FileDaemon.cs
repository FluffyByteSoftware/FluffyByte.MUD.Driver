/*
 * (FileDaemon.cs)
 *------------------------------------------------------------
 * Created - 11/24/2025 9:27:27 PM
 * Created by - Seliris
 *-------------------------------------------------------------
 */

using System.Text;
using FluffyByte.MUD.Driver.Core.Types.Daemons;
using FluffyByte.MUD.Driver.FluffyTools;

namespace FluffyByte.MUD.Driver.Core.Daemons;

/// <summary>Provides functionalities to manage and monitor the operational status of
/// a file management daemon.</summary>
/// <remarks>The FileDaemon class is a static utility for keeping track of the daemon's lifecycle
/// state, including its uptime and latest start time. It can be queried to get the current
/// status of the daemon.</remarks>
public static class FileDaemon
{
    private static string Name => "filed";
    
    #region Life Cycle
    /// <summary>Provides static methods for managing the lifecycle of the FileDaemon, which is responsible
    /// for overseeing file-related operations within the system. This class handles startup requests,
    /// state reporting, and integration with the system daemon for coordinated functionality.</summary>
    /// <remarks>The FileDaemon class is designed to operate as a static utility for managing file-specific tasks
    /// in conjunction with the SystemDaemon. Its primary responsibilities include initialization, runtime
    /// state maintenance, and status reporting. Thread safety for static members is
    /// ensured where applicable.</remarks>
    static FileDaemon()
    {
        SystemDaemon.GlobalBootRequest.Task.ContinueWith(_ => RequestStart());
    }
    
    private static CancellationTokenRegistration _shutdownRegistration;
    private static DateTime _lastStartTime = DateTime.MaxValue;
    private static TimeSpan Uptime => DateTime.UtcNow - _lastStartTime;
    
    private static DaemonStatus _state = DaemonStatus.Stopped;

    /// <summary>Initiates a request to start the FileDaemon, transitioning its state to the appropriate
    /// status and preparing it for operational activities. This action is intended to enable
    /// the daemon to perform its designated tasks and maintain its lifecycle.</summary>
    private static void RequestStart()
    {
        if (SystemDaemon.GlobalShutdownToken.IsCancellationRequested)
        {
            Log.Warn($"{Name}: RequestStart stopped due to shutdown.");
            return;
        }

        if (_state is DaemonStatus.Starting or DaemonStatus.Running)
        {
            Log.Warn($"{Name}: RequestStart stopped due to state: {_state}.");
            return;
        }

        try
        {
            _state = DaemonStatus.Starting;
            _lastStartTime = DateTime.UtcNow;

            _shutdownRegistration = new CancellationTokenRegistration();
            _shutdownRegistration = SystemDaemon.GlobalShutdownToken.Register(RequestStop);
            
            _state = DaemonStatus.Running;
        }
        catch (OperationCanceledException)
        {
            Log.Warn($"{Name}: RequestStart stopped due to shutdown.");
            _state = DaemonStatus.Error;
        }
        catch (Exception ex)
        {
            Log.Error($"{Name}: Exception in RequestStart()", ex);
            _state = DaemonStatus.Error;
        }
    }

    /// <summary>Initiates a request to stop the FileDaemon, transitioning its state to the appropriate status
    /// and halting its operations. This action is intended for orderly shutdown and resource cleanup.</summary>
    private static void RequestStop()
    {
        if (_state is DaemonStatus.Stopping or DaemonStatus.Stopped)
        {
            Log.Warn($"{Name}: RequestStop stopped due to state: {_state}.");
            return;
        }

        try
        {
            _state = DaemonStatus.Stopping;

            _shutdownRegistration.Dispose();
            _lastStartTime = DateTime.MaxValue;
            _state = DaemonStatus.Stopped;
        }
        catch (OperationCanceledException)
        {
            Log.Warn($"{Name}: RequestStop stopped due to shutdown.");
            _state = DaemonStatus.Error;
        }
        catch (Exception ex)
        {
            Log.Error($"{Name}: Exception in RequestStop()", ex);
            _state = DaemonStatus.Error;
        }
    }

    /// <summary>Retrieves the current operational status of the FileDaemon, including its name,
    /// status, uptime, and the last start time.</summary>
    /// <returns>A string containing the name of the daemon, its current status,
    /// the duration for which it has been running, and the last time it was started.</returns>
    internal static string RequestStatus()
    {
        var output = new StringBuilder();

        output.AppendLine($"{Name} -- {_state} -- (Running For: {Uptime} -- Started: {_lastStartTime})");
        
        return output.ToString();
    }
    #endregion Life Cycle
}

/*
*------------------------------------------------------------
* (FileDaemon.cs)
* See License.txt for licensing information.
*-----------------------------------------------------------
*/
