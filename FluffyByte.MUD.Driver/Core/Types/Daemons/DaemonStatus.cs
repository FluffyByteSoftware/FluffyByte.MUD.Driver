/*
 * (DaemonStatus.cs)
 *------------------------------------------------------------
 * Created - 11/23/2025 11:45:27 PM
 * Created by - Seliris
 *-------------------------------------------------------------
 */

namespace FluffyByte.MUD.Driver.Core.Types.Daemons;

/// <summary>
/// Specifies the operational status of a background daemon process.
/// </summary>
/// <remarks>Use this enumeration to represent and track the lifecycle state of a daemon, such as whether it is
/// running, stopped, starting, stopping, or has encountered an error. The status can be used to control process
/// management logic or to report the current state to monitoring systems.</remarks>
public enum DaemonStatus
{
    /// <summary>
    /// Indicates that the operation or process has been stopped and is no longer running.
    /// </summary>
    Stopped,
    /// <summary>
    /// Represents the initial or starting state.
    /// </summary>
    Starting,
    /// <summary>
    /// Indicates whether the current process or operation is running.
    /// </summary>
    Running,
    /// <summary>
    /// Indicates that the process or operation is in the process of stopping, but has not yet fully stopped.
    /// </summary>
    Stopping,
    /// <summary>
    /// Represents an error condition or state within the application.
    /// </summary>
    Error
}

/*
*------------------------------------------------------------
* (DaemonStatus.cs)
* See License.txt for licensing information.
*-----------------------------------------------------------
*/