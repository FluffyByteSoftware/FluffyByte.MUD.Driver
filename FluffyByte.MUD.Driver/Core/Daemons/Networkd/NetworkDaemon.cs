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

/// <summary>
/// Networkd handles all network related operations, through various subroutines and helpers.
/// </summary>
public static class NetworkDaemon
{
    /// <summary>
    /// The name of the network daemon (networkd)
    /// </summary>
    public static string Name => "networkd";

    /// <summary>
    /// Represents the current "State" of networkd
    /// </summary>
    public static DaemonStatus State { get; private set; }

    /// <summary>
    /// The present up time (as a TimeSpan) of the daemon
    /// </summary>
    public static TimeSpan Uptime => DateTime.UtcNow - _lastStartTime;

    private static DateTime _lastStartTime = DateTime.UtcNow;
    private static CancellationTokenRegistration _shutdownRegistration;
    
    /// <summary>
    /// Requests the networkd to start operations.
    /// </summary>
    public static async ValueTask RequestStart()
    {
        Log.Debug($"Startup called on {Name}.");
        
        if (State is DaemonStatus.Running or DaemonStatus.Starting)
        {
            Log.Warn($"A startup request was pushed to {Name} but it is in state: {State}. Omitting.");
            return;
        }

        State = DaemonStatus.Starting;

        try
        {
            _shutdownRegistration = SystemDaemon.GlobalShutdownToken.Register(OnShutdownInitiated);
            
            // Spin up the TcpWorker here
            _lastStartTime = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            Log.Error($"Error in RequestStart() of {Name}", ex);
            State = DaemonStatus.Error;
        }
        finally
        {
            State = DaemonStatus.Running;
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Requests the networkd to stop operations.
    /// </summary>
    public static async ValueTask RequestStop()
    {
        Log.Debug($"Shutdown called on {Name}.");
        if (State is DaemonStatus.Stopped or DaemonStatus.Stopping)
        {
            Log.Warn($"A shutdown request was pushed to {Name} but it is in state: {State}. Omitting.");
            return;
        }

        State = DaemonStatus.Stopping;

        try
        {
            // Handle graceful shutdown here

            await _shutdownRegistration.DisposeAsync();
        }
        catch (Exception ex)
        {
            Log.Error($"Error in RequestStop() of {Name}", ex);
            State = DaemonStatus.Error;
        }
        finally
        {
            if (State == DaemonStatus.Error) 
                await ValueTask.CompletedTask;
            
            State = DaemonStatus.Stopped;
        }
    }

    private static void OnShutdownInitiated()
    {
        Log.Debug($"Shutdown initiated on {Name}, Cancellation Method called successfully!");
    }
}
/*------------------------------------------------------------
* (NetworkDaemon.cs)
* See License.txt for licensing information.
*-----------------------------------------------------------
*/