/*
 * (TcpWorker.cs)
 *------------------------------------------------------------
 * Created - 12:23:35 AM
 * Created by - Jacob Chacko
 *------------------------------------------------------------
 */
using FluffyByte.MUD.Driver.Core.Types.Daemons;
using FluffyByte.MUD.Driver.FluffyTools;

namespace FluffyByte.MUD.Driver.Core.Daemons.NetworkD;

/// <summary>
/// TcpWorker is responsible for managing the TcpClient and Socket operations.
/// </summary>
public sealed class TcpWorker : IDaemonWorker
{
    /// <summary>
    /// The name of the worker (TcpWorker)
    /// </summary>
    public string Name => "TcpWorker";

    /// <summary>
    /// Represents the state the TcpWorker is presently in.
    /// </summary>
    public DaemonStatus State { get; private set; }

    /// <summary>
    /// The run time for the TcpWorker (as a TimeSpan)
    /// </summary>
    public TimeSpan Uptime => LastStartTime - DateTime.UtcNow;
    
    /// <summary>
    /// Returns the DateTime in UTC of the last start of TcpWorker.
    /// </summary>
    public DateTime LastStartTime { get; private set; }

    /// <summary>
    /// The shutdown event for this Client.
    /// </summary>
    private static CancellationTokenRegistration ShutdownRegistration { get; set; }
    
    /// <summary>
    /// Requests the TcpWorker to start operations.
    /// </summary>
    public void RequestStart()
    {
        if (State is DaemonStatus.Running or DaemonStatus.Starting)
        {
            Log.Warn($"{Name} was requested to start but is already in state: {State}");
            return;
        }

        State = DaemonStatus.Starting;

        try
        {
            ShutdownRegistration = SystemDaemon.GlobalShutdownToken.Register(RequestStop);
            
            LastStartTime = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            Log.Error($"Exception in RequestStart() or {Name}", ex);
            State = DaemonStatus.Error;
        }
        finally
        {
            if (State is not DaemonStatus.Error)
                State = DaemonStatus.Running;
        }
    }

    /// <summary>
    /// Request the TcpWorker to shutdown operations.
    /// </summary>
    public void RequestStop()
    {
        if (State is DaemonStatus.Stopped or DaemonStatus.Stopping)
        {
            Log.Warn($"{Name} was requested to stop but is already in state: {State}");
            return;
        }

        try
        {
            Log.Info($"{Name} is shutting down...");
            ShutdownRegistration.Dispose();
        }
        catch (OperationCanceledException)
        {
            Log.Debug($"{Name} graceful shutdown detected in {Name}");
        }
        catch (Exception ex)
        {
            Log.Debug($"{Name} was requested to stop and generated an exception.", ex);
            State = DaemonStatus.Error;
        }
        finally
        {
            if(State is not DaemonStatus.Error)
                State = DaemonStatus.Stopped;
        }
    }
    
}
/*
 *------------------------------------------------------------
 * (TcpWorker.cs)
 * See License.txt for licensing information.
 *-----------------------------------------------------------
 */