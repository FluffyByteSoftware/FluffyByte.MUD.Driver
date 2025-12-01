/*
 * (Constellations.cs)
 *------------------------------------------------------------
 * Created - 11/25/2025 9:24:40 PM
 * Created by - Seliris
 *-------------------------------------------------------------
 */
namespace FluffyByte.MUD.Driver.FluffyTools;

/// <summary>
/// Constellations is a singleton class that holds global constant values to make changing them much easier.
/// </summary>
public class Constellations
{
    /// <summary>Name of the Driver</summary>
    public const string DriverName = "FLuffyByte.MUD.Driver";
    
    private static readonly Lazy<Constellations> Singleton = new(() => new Constellations());
    /// <summary>
    /// Singleton Instance of Constellations
    /// </summary>
    public static Constellations Instance => Singleton.Value;

    /// <summary>
    /// Specifies the default threshold, in bytes, at which a flush operation is triggered. This value is set to 10
    /// megabytes.
    /// </summary>
    /// <remarks>Use this constant to determine when buffered data should be flushed to persistent storage or
    /// output. Adjusting the threshold may impact performance and memory usage depending on the application's
    /// requirements.</remarks>
    public const int FlushThresholdBytes = 10 * 1024 * 1024; // 10 MB
    
    /// <summary>Represents the name of the MUD (Multi-User Dungeon) instance.</summary>
    public const string Mudname = "ORC";

    /// <summary>
    /// Specifies the maximum number of socket connections that can be simultaneously handled by the system.
    /// This limit is used to control resource usage and ensure application stability under load.
    /// </summary>
    public const int MaximumNumberSockets = 25;

    /// <summary>Specifies the maximum size, in bytes, of a single message transmitted or received by the system.
    /// This value is commonly used to define buffer sizes for network operations. </summary>
    public const int MaximumMessageSize = 756;
    
    /// <summary>
    /// Designation for the local network adapter address to bind the TCP server to.
    /// </summary>
    public static string HostAddress => "10.0.0.84";
    /// <summary>
    /// Gets the default port number used with the HOST_ADDRESS for binding the TCP server.
    /// </summary>
    public static int HostPort => 9997;
    
    private static readonly string BaseDirectory = AppContext.BaseDirectory;
    
    /// <summary>
    /// Represents the default directory path where log files are stored.
    /// </summary>
    /// <remarks>The value is constructed by appending "Logs\" to the application's base directory. Use this
    /// constant when specifying locations for log file output to ensure consistency across the application.</remarks>
    public static readonly string LogDirectory = $@"{BaseDirectory}Logs\";
    /// <summary>
    /// Represents the full path to the application's data directory.
    /// </summary>
    /// <remarks>The value is constructed by combining the base directory with the "Data" subfolder. This path
    /// can be used to store or retrieve application-specific data files.</remarks>
    public static readonly string DataDirectory = $@"{BaseDirectory}Data\";
    /// <summary>
    /// Represents the full path to the application's configuration directory.
    /// </summary>
    /// <remarks>The value is constructed by appending "Config\" to the application's base directory. Use this
    /// constant to access or store configuration files in a standardized location.</remarks>
    public static readonly string ConfigDirectory = $@"{BaseDirectory}Config\";

    /// <summary>
    /// Private Singleton Constructor
    /// </summary>
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