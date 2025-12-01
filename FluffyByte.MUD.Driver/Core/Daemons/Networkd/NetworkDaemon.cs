/*
 * (NetworkDaemon.cs)
 *------------------------------------------------------------
 * Created - 11/26/2025 6:41:41 PM
 * Created by - Seliris
 *-------------------------------------------------------------
 */
using FluffyByte.MUD.Driver.Core.Types.Daemons;
using FluffyByte.MUD.Driver.FluffyTools;

namespace FluffyByte.MUD.Driver.Core.Daemons.NetworkD;

/// <summary>Networkd handles all network-related operations, through various subroutines and helpers.</summary>
public static class NetworkDaemon
{
    /// <summary>Represents the name of the NetworkDaemon instance.</summary>
    /// <remarks>The <c>Name</c> property provides the identifier of the NetworkDaemon, typically used
    /// for logging, diagnostics, or when interacting with other components within the system.
    /// It is a static and constant value initialized to "networkd".</remarks>
    private static string Name => "networkd";

    #region Workers
    /// <summary>Represents the central hub for managing connected clients within the networking subsystem.</summary>
    /// <remarks>
    /// The <c>Switchboard</c> property provides access to the instance responsible for tracking and managing
    /// client connections in the NetworkDaemon. It acts as a shared resource for managing communications,
    /// client addition, and removal in a thread-safe manner.
    /// </remarks>
    public static Switchboard? Switchboard { get; private set; } = new();

    /// <summary>Provides access to the global TCP socket instance managed by the NetworkDaemon.</summary>
    /// <remarks>
    /// The <c>TcpSocket</c> property represents the core TCP socket handler used by the NetworkDaemon
    /// to manage network connections. It facilitates the initialization and monitoring of TCP connections
    /// across the application and ensures proper resource management during the daemon's lifecycle.
    /// The property is initialized when the NetworkDaemon starts and is disposed of during shutdown,
    /// following global system cleanup routines.
    /// </remarks>
    private static TcpSocket? _tcpSocket;

    #endregion Workers
    
    #region Tick

    /// <summary>Executes a network tick for all connected clients, advancing internal states and processing any
    /// queued tasks.</summary>
    /// <param name="tickCount">The current server tick count, representing the total number of ticks since the
    /// server started.</param>
    /// <returns>An asynchronous task that represents the completion of the tick operation.</returns>
    public static async Task Tick(long tickCount)
    {
        if (Switchboard == null)
            return;

        await Switchboard.TickAllClients();
    }
    #endregion Tick
    
    #region Life Cycle
    private static CancellationTokenRegistration? _shutdownRegistration;
    
    /// <summary> Represents the current operational state of the NetworkDaemon.</summary>
    /// <remarks>The <c>State</c> property indicates the current lifecycle status of the NetworkDaemon instance,
    /// using the <c>DaemonStatus</c> enumeration. This property is primarily used to track and manage
    /// the daemon's runtime behavior, such as whether it is stopped, running, starting, stopping, or in an error state.
    /// </remarks>
    private static DaemonStatus _state = DaemonStatus.Stopped;

    /// <summary>Tracks the most recent time the NetworkDaemon was started.</summary>
    /// <remarks>The <c>_lastStartTime</c> field records the DateTime at which the NetworkDaemon entered the
    /// "Running" state. It is used internally for calculating the uptime of the daemon and remains
    /// private to ensure controlled access. By default, it is initialized to <c>DateTime.MaxValue</c>.</remarks>
    private static DateTime _lastStartTime = DateTime.MaxValue;

    /// <summary>Gets the total duration for which the NetworkDaemon has been running since its last start.</summary>
    /// <remarks>The <c>Uptime</c> property calculates and returns the elapsed time as a <c>TimeSpan</c>
    /// between the last recorded start time of the NetworkDaemon and the current UTC time. This value is
    /// dynamic and updates whenever accessed, providing a real-time view of the daemon's operational duration.
    /// If the daemon has never been started, the uptime will reflect an undefined or non-functional state.</remarks>
    private static TimeSpan Uptime => DateTime.UtcNow - _lastStartTime;

    /// <summary>Represents a summary of the current status of the NetworkDaemon.</summary>
    /// <remarks>The <c>RequestStatus</c> property provides a formatted string that combines the daemon's name,
    /// its current operational state (derived from the <c>DaemonStatus</c> enumeration), and its uptime.
    /// This property is useful for monitoring, diagnostics, and logging purposes, offering a concise
    /// overview of the NetworkDaemon's runtime status.</remarks>
    public static string RequestStatus => $"{Name} -- {_state} -- {Uptime}";

    static NetworkDaemon()
    {
        Log.Debug($"{Name}: Static constructor called.");
        
        _tcpSocket = new TcpSocket();
    }
    
    /// <summary>Requests the start of the network daemon. Updates the daemon's state and handles any errors or
    /// cancellation scenarios.</summary>
    /// <returns>An asynchronous operation representing the completion of the request to start the daemon.</returns>
    public static void RequestStart()
    {
        if (SystemDaemon.GlobalShutdownToken.IsCancellationRequested)
        {
            Log.Warn($"{Name}: RequestStart stopped due to shutdown.");
            return;
        }

        if (_state is DaemonStatus.Starting or DaemonStatus.Running)
        {
            Log.Warn($"{Name}: RequestStart already in progress or running, current state: {_state}");
        }

        try
        {
            Log.Debug($"{Name}: RequestStart() called.");
            _state = DaemonStatus.Starting;

            _shutdownRegistration = new CancellationTokenRegistration();
            _shutdownRegistration = SystemDaemon.GlobalShutdownToken.Register(RequestStop);

            Switchboard = new Switchboard();
            
            _tcpSocket = new TcpSocket();
            _tcpSocket.RequestStart();

            _lastStartTime = DateTime.UtcNow;
            _state = DaemonStatus.Running;
        }
        catch (OperationCanceledException)
        {
            // Expected during a shutdown
            Log.Debug($"{Name}: RequestStart canceled.  Is it a shutdown? : " +
                      $"{SystemDaemon.GlobalShutdownToken.IsCancellationRequested}");
        }
        catch (Exception ex)
        {
            Log.Error($"{Name}: Exception in RequestStart()", ex);
            
            // If you ever see a weird value for uptime that's negative - this is because it errored!
            _lastStartTime = DateTime.MinValue;
            
            _state = DaemonStatus.Error;
        }
        
        Log.Debug($"{Name}: RequestStart() completed successfully.");
    }

    /// <summary>Requests the cessation of the network daemon's operations.  Updates the daemon's state and handles
    /// the shutdown of internal components, ensuring proper cleanup and logging of any encountered errors.</summary>
    private static void RequestStop()
    {
        if (_state is DaemonStatus.Stopping or DaemonStatus.Stopped)
        {
            Log.Warn($"{Name}: RequestStop already in progress or stopped, current state: {_state}");
            return;
        }

        try
        {
            _state = DaemonStatus.Stopping;
            
            _lastStartTime = DateTime.MaxValue;
            _state = DaemonStatus.Stopped;
            
            _shutdownRegistration?.Dispose();
        }
        catch (Exception ex)
        {
            Log.Error($"{Name}: Exception in RequestStop()", ex);
            _state = DaemonStatus.Error;
        }
    }
    #endregion Life Cycle
}
/*------------------------------------------------------------
* (NetworkDaemon.cs)
* See License.txt for licensing information.
*-----------------------------------------------------------
*/