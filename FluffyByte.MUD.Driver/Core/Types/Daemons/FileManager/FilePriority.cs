/*
 * (FilePriority.cs)
 *------------------------------------------------------------
 * Created - 11/25/2025 7:25:50 PM
 * Created by - Seliris
 *-------------------------------------------------------------
 */

namespace FluffyByte.MUD.Driver.Core.Types.Daemons.FileManager;

/// <summary>
/// Represents the priority of queue to place this File into for writing.
/// </summary>
public enum FilePriority : byte
{
    /// <summary>
    /// The file should be written during the system fast heartbeat.
    /// </summary>
    SystemFast = 0,
    /// <summary>
    /// The file should be written during the system slow heartbeat.
    /// </summary>
    SystemSlow = 1,
    /// <summary>
    /// The file should be written during the game heartbeat.
    /// </summary>
    Game = 2
}

/*
*------------------------------------------------------------
* (FilePriority.cs)
* See License.txt for licensing information.
*-----------------------------------------------------------
*/