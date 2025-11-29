/*
 * (DaemonWorker.cs)
 *------------------------------------------------------------
 * Created - 9:12:53 PM
 * Created by - Jacob Chacko
 *------------------------------------------------------------
 */

namespace FluffyByte.MUD.Driver.Core.Types.Daemons;

/// <summary>
/// This interface is used in conjunction with any Daemon's subordinate processes.
/// </summary>
public interface IDaemonWorker
{
    /// <summary>
    /// The name of the worker
    /// </summary>
    string Name { get; }

    /// <summary>
    /// The current state of the worker.
    /// </summary>
    DaemonStatus State { get; }

    /// <summary>
    /// Returns the current run time of the daemon worker.
    /// </summary>
    TimeSpan Uptime { get; }

    /// <summary>
    /// The DateTime of the last start (in UTC)
    /// </summary>
    DateTime LastStartTime { get; }

    /// <summary>
    /// Request the worker to start its internal processes.
    /// </summary>
    void RequestStart();

    /// <summary>
    /// Request the worker to stop its internal processes.
    /// </summary>
    void RequestStop();
}
/*
 *------------------------------------------------------------
 * (DaemonWorker.cs)
 * See License.txt for licensing information.
 *-----------------------------------------------------------
 */