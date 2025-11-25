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

public static class FileDaemon
{
    private static readonly Heartbeat SystemFastHeartbeat = new(TimeSpan.FromMilliseconds(1000), SystemFastTick);
    
    public static async Task RequestStart()
    {
        await SystemFastHeartbeat.Start();
    }

    public static async Task RequestStop()
    {
        await SystemFastHeartbeat.Stop();
    }

    private static async Task SystemFastTick(long tick)
    {
        Log.Info($"SystemFastTick called.");

        await SystemFastHeartbeat.Start();

        await Task.CompletedTask;
    }
}

/*
*------------------------------------------------------------
* (FileDaemon.cs)
* See License.txt for licensing information.
*-----------------------------------------------------------
*/