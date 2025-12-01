/*
 * (HeartbeatDaemon.cs)
 *------------------------------------------------------------
 * Created - 11:45:59 PM
 * Created by - Jacob Chacko
 *------------------------------------------------------------
 */

using FluffyByte.MUD.Driver.Core.Types.Daemons;
using FluffyByte.MUD.Driver.FluffyTools;

namespace FluffyByte.MUD.Driver.Core.Daemons;

/// <summary>The HeartbeatDaemon is a static class responsible for managing recurring
/// heartbeat operations within a system. It provides the mechanism to
/// invoke periodic tasks that are essential for maintaining the health and
/// consistency of processes in the application.</summary>
/// <remarks>This class is part of the FluffyByte.MUD.Driver.Core.Daemons namespace
/// and is primarily intended for internal use within the system to support
/// regular maintenance operations.</remarks>
public static class HeartbeatDaemon
{
    private static string Name => $"hbeatd";
    
    #region Life Cycle
    private static CancellationTokenRegistration _shutdownRegistration;
    
    private static DaemonStatus _state;

    static HeartbeatDaemon()
    {
        SystemDaemon.GlobalBootRequest.Task.ContinueWith(_ =>
        {
            Log.Debug($"{Name}: GlobalBootRequest called.");
            RequestStart();
        });

        _shutdownRegistration = SystemDaemon.GlobalShutdownToken.Register(RequestShutdown);
        
        _state = DaemonStatus.Stopped;
    }

    /// <summary>Initiates a request to start the HeartbeatDaemon process.
    /// This method handles the transition from the current state to the starting state
    /// and performs necessary preparations for daemon execution.</summary>
    /// <returns>A task that represents the asynchronous operation of initiating the
    /// start process.</returns>
    private static void RequestStart()
    {
        if (SystemDaemon.GlobalShutdownToken.IsCancellationRequested)
        {
            Log.Warn($"{Name}: RequestStart stopped due to shutdown.");
            return;
        }

        if (_state is DaemonStatus.Running or DaemonStatus.Starting)
        {
            Log.Warn($"{Name}: RequestStart already in progress or stopped, current state: {_state}");
            return;
        }

        try
        {
            _state = DaemonStatus.Starting;

            _shutdownRegistration = SystemDaemon.GlobalShutdownToken.Register(RequestShutdown);
            
            _state = DaemonStatus.Running;
        }
        catch (OperationCanceledException)
        {
            // Expected during a shutdown
            Log.Warn($"OperationCanceled during RequestStart of HeartbeatDaemon.");
            _state = DaemonStatus.Error;
        }
        catch (Exception ex)
        {
            Log.Error($"Exception during RequestStart of HeartbeatDaemon.", ex);
            _state = DaemonStatus.Error;
        }
    }

    private static void RequestShutdown()
    {
        if (_state is DaemonStatus.Stopping or DaemonStatus.Stopped)
        {
            Log.Warn($"{Name} RequestShutdown already in progress or stopped, current state: {_state}");
            return;
        }

        try
        {
            _state = DaemonStatus.Stopping;
            
            _shutdownRegistration.Dispose();
            
            _state = DaemonStatus.Stopped;
        }
        catch (OperationCanceledException)
        {
            // Expected during a shutdown
            Log.Warn($"OperationCanceled during RequestShutdown of HeartbeatDaemon.");
            _state = DaemonStatus.Error;
        }
        catch(Exception ex)
        {
            Log.Error($"Exception during RequestShutdown of HeartbeatDaemon.", ex);
            _state = DaemonStatus.Error;
        }
    }
    #endregion Life Cycle

}
/*
 *------------------------------------------------------------
 * (HeartbeatDaemon.cs)
 * See License.txt for licensing information.
 *-----------------------------------------------------------
 */