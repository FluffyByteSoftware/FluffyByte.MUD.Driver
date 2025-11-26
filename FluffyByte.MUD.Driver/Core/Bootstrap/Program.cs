/*
 * (Program.cs)
 *------------------------------------------------------------
 * Created - 11/19/2025 7:40:24 PM
 * Created by - Seliris
 *-------------------------------------------------------------
 */

using System.Diagnostics;
using System.Text;
using FluffyByte.MUD.Driver.Core.Daemons;
using FluffyByte.MUD.Driver.Core.Types.Daemons.FileManager;
using FluffyByte.MUD.Driver.FluffyTools;

namespace FluffyByte.MUD.Driver.Core.Bootstrap;

/// <summary>
/// Provides the entry point and application metadata for the FluffyByte MUD driver.
/// </summary>
public static class Program
{
    /// <summary>
    /// Gets the name of the driver component.
    /// </summary>
    public const string Name = "FluffyByte.MUD.Driver";

    /// <summary>
    /// Gets the current version of the application or library.
    /// </summary>
    public const string Version = "0.0.1a";

    /// <summary>
    /// Serves as the entry point for the application.
    /// </summary>
    /// <param name="args">An array of command-line arguments supplied to the application. Can be empty if no arguments are provided.</param>
    /// <returns>A task that represents the asynchronous operation of the application entry point.</returns>
    public static async Task Main(string[] args)
    { 
        if(args.Length > 0)
        {
            Console.WriteLine($"Args: {args}");
            
        }

        Log.Info($"{Name}.{Version} starting up...");

        Thread.Sleep(1000);

        await SystemDaemon.RequestStart();


        byte[]? fileData = await FileDaemon.IO.Read(@"E:\Temp\test.txt", FilePriority.Game);

        if(fileData == null || fileData.Length == 0)
        {
            Log.Warn($@"E:\Temp\test.txt was empty.");
        }
        else
        {
            Log.DisplayFileContents(UTF8Encoding.UTF8.GetString(fileData));
        }

        byte[] writeThis = UTF8Encoding.UTF8.GetBytes("This is a test write at " + DateTime.UtcNow.ToString());

        await FileDaemon.IO.Write(@"E:\Temp\test.txt", writeThis, FilePriority.Game);

        Console.WriteLine($"{SystemDaemon.RequestStatus()}");

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