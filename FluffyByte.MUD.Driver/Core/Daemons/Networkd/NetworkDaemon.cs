/*
 * (NetworkDaemon.cs)
 *------------------------------------------------------------
 * Created - 11/26/2025 6:41:41 PM
 * Created by - Seliris
 *-------------------------------------------------------------
 */

namespace FluffyByte.MUD.Driver.Core.Daemons.Networkd;

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
    public static TcpWorker TcpWorker { get; private set; }

    /// <summary>
    ///
    /// </summary>
    public static Switchboard Switchboard { get; private set; }

    /// <summary>
    /// Constructor for networkd
    /// </summary>
    static NetworkDaemon()
    {
        TcpWorker = new();
        Switchboard = new();
    }

    /// <summary>
    /// Starts the networkd daemon.
    /// </summary>
    public static async ValueTask RequestStart()
    {
        await TcpWorker.RequestStart();
    }
}
/*------------------------------------------------------------
* (NetworkDaemon.cs)
* See License.txt for licensing information.
*-----------------------------------------------------------
*/
