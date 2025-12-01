/*
 * (Program.cs)
 *------------------------------------------------------------
 * Created - 11/19/2025 7:40:24 PM
 * Created by - Seliris
 *-------------------------------------------------------------
 */
using FluffyByte.MUD.Driver.Core.Daemons;
using FluffyByte.MUD.Driver.FluffyTools;

namespace FluffyByte.MUD.Driver.Core.Bootstrap;

/// <summary>
/// Provides the entry point and application metadata for the FluffyByte MUD driver.
/// </summary>
public static class Program
{
    /// <summary>The entry point method for the FluffyByte MUD driver application.
    /// Responsible for initializing the core system components, handling command-line arguments,
    /// logging startup messages, and managing the lifecycle of the SystemDaemon.</summary>
    /// <param name="args">An array of command-line arguments passed to the application at runtime.</param>
    /// <returns>A Task representing the asynchronous execution of the application's main process.</returns>
    public static async Task Main(string[] args)
    {
        if (args.Length > 0)
        {
            Console.WriteLine($"Args: {args}");
        }
        
        BootstrapLogger.Write($"Current Time: {DateTime.UtcNow}... Preparing to boot driver.");
        
        Thread.Sleep(millisecondsTimeout:1000);

        BootstrapLogger.Write("Starting systemd");
        
        await SystemDaemon.RequestStart();
        
        Log.Info(SystemDaemon.RequestStatus());
        
        Console.ReadLine();

        await SystemDaemon.RequestShutdown();

        Console.ReadLine();
    }
}

/*
*------------------------------------------------------------
* (Program.cs)
* See License.txt for licensing information.
*-----------------------------------------------------------
*/