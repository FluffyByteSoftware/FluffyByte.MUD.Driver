/*
 * (Program.cs)
 *------------------------------------------------------------
 * Created - 11/19/2025 7:40:24 PM
 * Created by - Seliris
 *-------------------------------------------------------------
 */

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

        Log.Info($"{Constellations.DriverName} starting up...");
        
        Thread.Sleep(millisecondsTimeout:1000);

        await SystemDaemon.RequestStart();
        
        // Store some data
        var fileData = await FileDaemon.InputOutput.Read(@"E:\Temp\test.txt", FilePriority.SystemFast);

        if(fileData == null || fileData.Length == 0)
        {
            Log.Warn($@"E:\Temp\test.txt was empty.");
        }
        else
        {
            var contents = Encoding.UTF8.GetString(fileData);
            Log.DisplayFileContents(contents);
        }
        
        var timeStamp = DateTime.UtcNow.ToString("F.nnn");
        var messageToEncode = $"This is a test write at {timeStamp}";
        
        var writeThis = Encoding.UTF8.GetBytes(messageToEncode);
        
        FileDaemon.InputOutput.Write(@"E:\Temp\test.txt", writeThis, FilePriority.SystemFast);
        
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