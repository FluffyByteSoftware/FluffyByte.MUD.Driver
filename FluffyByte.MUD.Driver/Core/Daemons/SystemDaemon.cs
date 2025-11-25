/*
 * (SystemDaemon.cs)
 *------------------------------------------------------------
 * Created - 11/23/2025 11:25:01 PM
 * Created by - Seliris
 *-------------------------------------------------------------
 */

using System.Runtime.CompilerServices;
using FluffyByte.MUD.Driver.Core.Daemons.FileManager;
using FluffyByte.MUD.Driver.Core.Types.Daemons;
using FluffyByte.MUD.Driver.FluffyTools;

namespace FluffyByte.MUD.Driver.Core.Daemons;

public static class SystemDaemon
{
    public static CancellationToken GlobalShutdownToken { get; private set; }

    public static string Name => "systemd";

    public static DaemonStatus Status { get; private set; }

    public static async ValueTask RequestStart()
    {
        Log.Debug($"Initialization called on {Name}.");
        
        GlobalShutdownToken = new();

        await FileDaemon.RequestStart();
        Log.Debug($"GlobalShutdownToken registered.");
    }

    public static async ValueTask RequestStop()
    {
        Log.Debug($"Shutdown called on {Name}.");

        CancellationTokenSource _cts = CancellationTokenSource.CreateLinkedTokenSource(GlobalShutdownToken);

        await _cts.CancelAsync();

        await FileDaemon.RequestStop();

        Log.Debug($"GlobalShutdownToken cancelled.");
    }
}

/*
*------------------------------------------------------------
* (SystemDaemon.cs)
* See License.txt for licensing information.
*-----------------------------------------------------------
*/