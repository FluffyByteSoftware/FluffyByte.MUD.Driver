/*
 * (Constellations.cs)
 *------------------------------------------------------------
 * Created - 11/25/2025 9:24:40 PM
 * Created by - Seliris
 *-------------------------------------------------------------
 */

using FluffyByte.MUD.Driver.Core.Bootstrap;

namespace FluffyByte.MUD.Driver.FluffyTools;

/// <summary>
/// Constellations is a singleton class that holds global defined constants, to make changing them much easier.
/// </summary>
public class Constellations
{
    private readonly static Lazy<Constellations> _instance = new(() => new());
    /// <summary>
    /// Singleton Instance of Constellations
    /// </summary>
    public static Constellations Instance => _instance.Value;

    private readonly static string _baseDirectory = AppContext.BaseDirectory;

    /// <summary>
    /// Specifies the default threshold, in bytes, at which a flush operation is triggered. This value is set to 10
    /// megabytes.
    /// </summary>
    /// <remarks>Use this constant to determine when buffered data should be flushed to persistent storage or
    /// output. Adjusting the threshold may impact performance and memory usage depending on the application's
    /// requirements.</remarks>
    public const int FLUSHTHRESHOLD_BYTES = 10 * 1024 * 1024; // 10 MB
    /// <summary>
    /// Represents the name of the MUD (Multi-User Dungeon) instance.
    /// </summary>
    public const string MUDNAME = "ORC";

    /// <summary>
    /// Designation for the local network adapter address to bind the TCP server to.
    /// </summary>
    public static string HOST_ADDRESS => "10.0.0.84";
    /// <summary>
    /// Gets the default port number used with the HOST_ADDRESS for binding the TCP server.
    /// </summary>
    public static int HOST_PORT => 9997;

    /// <summary>
    /// Represents the default directory path where log files are stored.
    /// </summary>
    /// <remarks>The value is constructed by appending "Logs\" to the application's base directory. Use this
    /// constant when specifying locations for log file output to ensure consistency across the application.</remarks>
    public readonly static string LOG_DIRECTORY = $@"{_baseDirectory}Logs\";
    /// <summary>
    /// Represents the full path to the application's data directory.
    /// </summary>
    /// <remarks>The value is constructed by combining the base directory with the "Data" subfolder. This path
    /// can be used to store or retrieve application-specific data files.</remarks>
    public readonly static string DATA_DIRECTORY = $@"{_baseDirectory}Data\";
    /// <summary>
    /// Represents the full path to the application's configuration directory.
    /// </summary>
    /// <remarks>The value is constructed by appending "Config\" to the application's base directory. Use this
    /// constant to access or store configuration files in a standardized location.</remarks>
    public readonly static string CONFIG_DIRECTORY = $@"{_baseDirectory}Config\";

    private Constellations()
    {
    }
}

/*
*------------------------------------------------------------
* (Constellations.cs)
* See License.txt for licensing information.
*-----------------------------------------------------------
*/