/*
 * (FileDaemon.cs)
 *------------------------------------------------------------
 * Created - 11/24/2025 9:27:27 PM
 * Created by - Seliris
 *-------------------------------------------------------------
 */

using FluffyByte.MUD.Driver.Core.Types.Heartbeats;
using FluffyByte.MUD.Driver.FluffyTools;

namespace FluffyByte.MUD.Driver.Core.Daemons.FileManager;

/// <summary>
/// Represents the FileDaemon which is responsible for managing file-related operations for the system.
/// </summary>
public static class FileDaemon
{
    private static Heartbeat? _systemFastHeartbeat;
    private static Heartbeat? _systemSlowHeartbeat;
    private static Heartbeat? _gameHeartbeat;

    /// <summary>
    /// Initializes and starts the system fast heartbeat asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation of starting the system fast heartbeat.</returns>
    public static async Task RequestStart()
    {
        _systemFastHeartbeat = new(TimeSpan.FromMilliseconds(1500), SystemFastTick);
        _systemSlowHeartbeat = new(TimeSpan.FromMilliseconds(3000), SystemSlowTick);
        _gameHeartbeat = new(TimeSpan.FromMilliseconds(3500), GameTick);

        await _systemFastHeartbeat.Start();
        await _systemSlowHeartbeat.Start();
        await _gameHeartbeat.Start();
    }

    /// <summary>
    /// Requests a stop of the system fast heartbeat asynchronously.
    /// </summary>
    /// <remarks>If the system fast heartbeat is not initialized, the method logs a warning and does not
    /// perform the stop operation.</remarks>
    /// <returns>A task that represents the asynchronous stop operation.</returns>
    public static async Task RequestStop()
    {
        if(_systemFastHeartbeat == null || _systemSlowHeartbeat == null || _gameHeartbeat == null)
        {
            Log.Warn($"A heartbeat timer was null.");
            return;
        }


        await _systemFastHeartbeat.Stop();
        await _systemSlowHeartbeat.Stop();
        await _gameHeartbeat.Stop();
    }

    /// <summary>
    /// Performs a fast system tick operation for the specified tick value asynchronously.
    /// </summary>
    /// <param name="tick">The tick value representing the current system tick to process.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private static async Task SystemFastTick(long tick)
    {
        Log.Info($"SystemFastTick called.");

        await Task.CompletedTask;
    }

    private static async Task SystemSlowTick(long tick)
    {
        Log.Info($"SystemSlowTick called.");
    }

    private static async Task GameTick(long tick)
    {
        Log.Info($"Game Tick called.");
    }
}

/*
*------------------------------------------------------------
* (FileDaemon.cs)
* See License.txt for licensing information.
*-----------------------------------------------------------
*/