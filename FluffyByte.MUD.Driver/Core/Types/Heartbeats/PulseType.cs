/*
 * (PulseType.cs)
 *------------------------------------------------------------
 * Created - 11/24/2025 6:21:54 PM
 * Created by - Seliris
 *-------------------------------------------------------------
 */

namespace FluffyByte.MUD.Driver.Core.Types.Heartbeats;

/// <summary>
/// Specifies the type of periodic pulse used to schedule background or game loop operations.
/// </summary>
/// <remarks>Use this enumeration to select the appropriate timing strategy for system or game-related tasks.
/// 'SysFast' is intended for frequent, lightweight operations such as caching or quick I/O checks. 'SysSlow' is
/// suitable for less frequent, resource-intensive tasks like disk flushing or garbage collection. 'Game' represents the
/// main synchronous game tick, typically driven by the primary game loop at a fixed interval (such as 100 ms).
/// Selecting the correct pulse type helps optimize performance and resource usage.</remarks>
public enum PulseType
{
    /// <summary>
    /// High frequency background operations (Caching, fast I/O checks)
    /// Suggested: 500 ms - 1s.
    /// </summary>
    SysFast,
    /// <summary>
    /// Low frequency background operations (Disk flush, garbage collection)
    /// </summary>
    SysSlow,
    /// <summary>
    /// The Synchronous World Tick. STRICTLY 100 ms (or target tick rate).
    /// Driven solely by the Main Game Loop.
    /// </summary>
    Game
}

/*
*------------------------------------------------------------
* (PulseType.cs)
* See License.txt for licensing information.
*-----------------------------------------------------------
*/