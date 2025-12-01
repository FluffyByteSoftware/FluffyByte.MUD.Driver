/*
 * (CommandDaemon.cs)
 *------------------------------------------------------------
 * Created - 3:53:09 PM
 * Created by - Jacob Chacko
 *------------------------------------------------------------
 */

using System.Text;
using FluffyByte.MUD.Driver.Core.Types.Daemons;
using FluffyByte.MUD.Driver.FluffyTools;

namespace FluffyByte.MUD.Driver.Core.Daemons.NetworkD;

/// <summary>Provides a daemon for managing network-related commands in the system.
/// Responsible for processing client commands, tracking its operational state, and managing uptime.</summary>
public static class CommandDaemon
{
    private static string Name => "cmdd";

    #region Life Cycle

    private static CancellationTokenRegistration? _shutdownRegistration;
    private static DaemonStatus _state = DaemonStatus.Stopped;
    private static DateTime _lastStartTime = DateTime.MaxValue;
    private static TimeSpan Uptime => DateTime.UtcNow - _lastStartTime;

    static CommandDaemon()
    {
        _state = DaemonStatus.Stopped;
        _shutdownRegistration = null;
    }

    /// <summary>Attempts to start the CommandDaemon by transitioning its state to starting,
    /// registering a shutdown callback, and logging any exceptions encountered during the process.
    /// This method ensures proper handling of system shutdown signals to prevent operations when the system is
    /// shutting down.</summary>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled due to a global
    /// shutdown signal.</exception>
    /// <exception cref="Exception">Thrown when an unexpected error occurs during the startup process.</exception>
    public static void RequestStart()
    {
        if (SystemDaemon.GlobalShutdownToken.IsCancellationRequested)
        {
            Log.Warn($"{Name}: RequestStart stopped due to shutdown.");
            return;
        }

        try
        {
            _state = DaemonStatus.Starting;

            _shutdownRegistration = new CancellationTokenRegistration();

            _shutdownRegistration = SystemDaemon.GlobalShutdownToken.Register(RequestStop);

            _lastStartTime = DateTime.UtcNow;

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

    private static void RequestStop()
    {
        if (_state is DaemonStatus.Stopping or DaemonStatus.Stopped)
        {
            Log.Warn($"{Name}: RequestStop stopped due to state; {_state}");
            return;
        }

        try
        {
            _state = DaemonStatus.Stopping;

            _lastStartTime = DateTime.MaxValue;

            _shutdownRegistration?.Dispose();

            _state = DaemonStatus.Stopped;
        }
        catch (OperationCanceledException)
        {
            // Expected during an abrupt shutdown
            Log.Warn($"{Name}: RequestStop stopped due to shutdown.");
            _state = DaemonStatus.Error;
        }
        catch (Exception ex)
        {
            Log.Error($"{Name}: Exception in RequestStop()", ex);
            _state = DaemonStatus.Error;
        }
    }

    /// <summary>
    /// Provides a human-readable summary of the daemon's current state, including its name,
    /// operational status, and the uptime duration since it last started.
    /// </summary>
    /// <remarks>This property combines the daemon's name, its current <see cref="DaemonStatus"/>,
    /// and the amount of time it has been running since the last start. Useful for logging, monitoring, or diagnostic
    /// purposes to track the operational state and performance of the daemon.</remarks>
    public static string RequestStatus => $"{Name} -- {_state} -- {Uptime}";

    #endregion Life Cycle

    #region Tick Operations

    /// <summary>Processes commands for all connected clients by dequeuing and handling client commands.
    /// This method logs the command and sends a response back to the client.</summary>
    /// <param name="tickCount">The current tick count, used to identify the state of the system when the method
    /// is called.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async Task Tick(long tickCount)
    {
        if (NetworkDaemon.Switchboard == null)
            return;

        try
        {
            foreach (var client in NetworkDaemon.Switchboard.AllClients)
            {
                while (client.DequeueCommand() is { } command)
                {
                    var commandText = Encoding.UTF8.GetString(command);

                    Log.Info($"{client.Name}: {commandText}");

                    client.QueueWrite($"You said: {commandText}\r\n");
                    client.SetPrompt("> ");
                }
            }

            await Task.CompletedTask;
        }
        catch (OperationCanceledException)
        {
            // Expected during a shutdown
            Log.Warn($"{Name}: Tick stopped due to shutdown.");
        }
        catch (Exception ex)
        {
            Log.Error($"{Name}: Exception during Tick.", ex);
        }
    }
    #endregion Tick Operations
}
/*
 *------------------------------------------------------------
 * (CommandDaemon.cs)
 * See License.txt for licensing information.
 *-----------------------------------------------------------
 */