/*
 * (Constellations.cs)
 *------------------------------------------------------------
 * Created - 11/25/2025 9:24:40 PM
 * Created by - Seliris
 *-------------------------------------------------------------
 */

using FluffyByte.MUD.Driver.Core.Bootstrap;

namespace FluffyByte.MUD.Driver.FluffyTools;

public class Constellations
{
    private readonly static Lazy<Constellations> _instance = new(() => new());
    public static Constellations Instance => _instance.Value;

    private readonly static string _baseDirectory = AppContext.BaseDirectory;

    public const int FLUSHTHRESHOLD_BYTES = 10 * 1024 * 1024; // 10 MB
    public const string MUDNAME = "ORC";

    public readonly static string LOG_DIRECTORY = $@"{_baseDirectory}Logs\";
    public readonly static string DATA_DIRECTORY = $@"{_baseDirectory}Data\";
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