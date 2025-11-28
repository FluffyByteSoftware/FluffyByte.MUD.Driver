/*
 * (NetworkDaemon.cs)
 *------------------------------------------------------------
 * Created - 11/26/2025 6:41:41 PM
 * Created by - Seliris
 *-------------------------------------------------------------
 */

namespace FluffyByte.MUD.Driver.Core.Daemons.NetworkD;

/// <summary>
/// Networkd handles all network related operations, through various subroutines and helpers.
/// </summary>
public static class NetworkDaemon
{
    /// <summary>
    /// The name of the networkdaemon (networkd)
    /// </summary>
    public static string Name => "networkd";

    /// <summary>
    /// The TcpWorker handles listening for TCP Connections.
    /// It will pass authenticated connections to the Switchboard for further processing.
    /// </summary>
    private static readonly TcpWorker TcpWorker;

    /// <summary>
    /// The switchboard handles routing messages from the game to the appropriate client.
    /// </summary>
    public static Switchboard Switchboard { get; private set; }

    /// <summary>
    /// Constructor for the network daemon
    /// </summary>
    static NetworkDaemon()
    {
        TcpWorker = new TcpWorker();
        Switchboard = new Switchboard();
    }

    /// <summary>
    /// Starts the networkd daemon.
    /// </summary>
    public static async ValueTask RequestStart()
    {
        await TcpWorker.RequestStart();
        await Switchboard.RequestStart();
    }
}
/*------------------------------------------------------------
* (NetworkDaemon.cs)
* See License.txt for licensing information.
*-----------------------------------------------------------
*/