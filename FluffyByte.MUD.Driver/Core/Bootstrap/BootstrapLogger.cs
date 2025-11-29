/*
 * (BootstrapLogger.cs)
 *------------------------------------------------------------
 * Created - 1:04:53 PM
 * Created by - Jacob Chacko
 *------------------------------------------------------------
 */
using FluffyByte.MUD.Driver.FluffyTools;

namespace FluffyByte.MUD.Driver.Core.Bootstrap;

internal static class BootstrapLogger
{
    private const string LOG_FILE_PATH = @".\Logs\LastBootstrap.log";

    public static void Write(string message)
    {
        CheckLastLogSize();
        
        Console.WriteLine(message);
        
        File.AppendAllText(LOG_FILE_PATH, message);
    }

    private static void CheckLastLogSize()
    {
        try
        {
            Directory.CreateDirectory((@".\Logs\"));

            using (File.Create(LOG_FILE_PATH))
            {
                if (new FileInfo(LOG_FILE_PATH).Length > 1024)
                    File.Move(LOG_FILE_PATH, @".\Logs\{DateTime.UtcNow}_bootstrap.log");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
    }
}
/*
 *------------------------------------------------------------
 * (BootstrapLogger.cs)
 * See License.txt for licensing information.
 *-----------------------------------------------------------
 */