/*
 * (HeartbeatDaemon.cs)
 *------------------------------------------------------------
 * Created - 11:45:59 PM
 * Created by - Jacob Chacko
 *------------------------------------------------------------
 */
using System.Net.Sockets;
using FluffyByte.MUD.Driver.Core.Daemons.Networking;
using FluffyByte.MUD.Driver.Core.Types.Daemons;
using FluffyByte.MUD.Driver.Core.Types.Heartbeats;
using FluffyByte.MUD.Driver.Core.Daemons.Commands;
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
    private static CancellationTokenRegistration? _shutdownRegistration;
    private static DaemonStatus _state;

    private static DateTime _lastStartTime = DateTime.MaxValue;
    private static TimeSpan Uptime => DateTime.UtcNow - _lastStartTime;

    /// <summary>Provides core functionality for managing the HeartbeatDaemon process, including lifecycle
    /// management, status tracking, and integration with the global system daemon behavior.</summary>
    static HeartbeatDaemon()
    {
        _filedHeartbeat = new Heartbeat(TimeSpan.FromSeconds(5), TickFileDaemon);
        _networkdHeartbeat = new Heartbeat(TimeSpan.FromMilliseconds(750), NetworkDaemon.Tick);
        _cmdHeartbeat = new Heartbeat(TimeSpan.FromMilliseconds(500), CommandDaemon.Tick);
        
        _state = DaemonStatus.Stopped;
    }

    /// <summary>Initiates a request to start the HeartbeatDaemon process. This method handles the transition
    /// from the current state to the starting state and performs necessary preparations for daemon
    /// execution.</summary>
    /// <returns>A task that represents the asynchronous operation of initiating the start process.</returns>
    public static async Task RequestStart()
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
            Log.Debug($"{Name}: RequestStart() called.");
            
            _state = DaemonStatus.Starting;

            _shutdownRegistration = new CancellationTokenRegistration();
            _shutdownRegistration = SystemDaemon.GlobalShutdownToken.Register(RequestShutdown);
            
            _lastStartTime = DateTime.UtcNow;
            
            _filedHeartbeat = new Heartbeat(TimeSpan.FromSeconds(3), TickFileDaemon);
            _networkdHeartbeat = new Heartbeat(TimeSpan.FromMilliseconds(1500), NetworkDaemon.Tick);
            _cmdHeartbeat = new Heartbeat(TimeSpan.FromMilliseconds(500), CommandDaemon.Tick);
            
            await _filedHeartbeat.Start();
            await _networkdHeartbeat.Start();
            await _cmdHeartbeat.Start();
            
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

        if (_state is DaemonStatus.Running)
        {
            Log.Debug($"{Name}: RequestStart() completed successfully.");
        }
    }

    /// <summary>Initiates the shutdown process for the HeartbeatDaemon. This method transitions the daemon from its
    /// current state to the stopping state, performs cleanup, and ensures the daemon is properly stopped.
    /// In case of an error or cancellation during shutdown, the daemon's state is set to indicate an error.</summary>
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
            _lastStartTime = DateTime.MaxValue;
            
            _shutdownRegistration?.Dispose();
            _state = DaemonStatus.Stopped;
        }
        catch (SocketException)
        {
            // Expected during a shutdown
            Log.Warn($"SocketException during RequestShutdown of HeartbeatDaemon.");
            _state = DaemonStatus.Error;
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
    
    internal static string RequestStatus => $"{Name} -- {_state} -- {Uptime}";
    #endregion Life Cycle
    
    #region File Daemon Ticks
    private static async Task TickFileDaemon(long tickCount)
    {
        try
        {
            await FileDaemon.Tick(tickCount);
        }
        catch (OperationCanceledException)
        {
            // Expected during an abrupt shutdown
            Log.Debug($"OperationCanceled during FileDaemon Tick.");
        }
        catch (Exception ex)
        {
            Log.Error($"Exception during FileDaemon Tick.", ex);
        }
        await Task.CompletedTask;
    }
    #endregion File Daemon Ticks
    
    #region Heartbeats
    private static Heartbeat _networkdHeartbeat;
    private static Heartbeat _filedHeartbeat;
    private static Heartbeat _cmdHeartbeat;

    #endregion Heartbeats
}
/*
 *------------------------------------------------------------
 * (HeartbeatDaemon.cs)
 * See License.txt for licensing information.
 *-----------------------------------------------------------
 */