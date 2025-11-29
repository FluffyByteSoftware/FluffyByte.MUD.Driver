/*
 * (Program.cs)
 *------------------------------------------------------------
 * Created - 11/19/2025 7:40:24 PM
 * Created by - Seliris
 *-------------------------------------------------------------
 */
using FluffyByte.MUD.Driver.Core.Daemons;
using FluffyByte.MUD.Driver.Core.Types.Daemons;
using FluffyByte.MUD.Driver.FluffyTools;

namespace FluffyByte.MUD.Driver.Core.Bootstrap;

/// <summary>
/// Provides the entry point and application metadata for the FluffyByte MUD driver.
/// </summary>
public static class Program
{
    /// <summary>
    /// Serves as the entry point for the application.
    /// </summary>
    /// <param name="args">An array of command-line arguments supplied to the application. Can be empty if
    /// no arguments are provided.</param>
    /// <returns>A task that represents the asynchronous operation of the application entry point.</returns>
    public static async Task Main(string[] args)
    { 
        if(args.Length > 0)
        {
            Console.WriteLine($"Args: {args}");
        }
        
        BootstrapLogger.Write($"Current Time: {DateTime.UtcNow}... Preparing to boot driver.");
        
        Thread.Sleep(millisecondsTimeout:1000);

        BootstrapLogger.Write("Starting systemd");
        await SystemDaemon.RequestStart();
        
        if(SystemDaemon.State == DaemonStatus.Running)
            BootstrapLogger.Write("Systemd is now running.  Transitioning to Log.");

        Log.Info($"Bootstrap sequence completed.");
        Log.Info($"{SystemDaemon.RequestStatus()}");
        
        Console.ReadLine();

        await SystemDaemon.RequestStop();

        Console.ReadLine();
    }
}

/*
*------------------------------------------------------------
* (Program.cs)
* See License.txt for licensing information.
*-----------------------------------------------------------
*/