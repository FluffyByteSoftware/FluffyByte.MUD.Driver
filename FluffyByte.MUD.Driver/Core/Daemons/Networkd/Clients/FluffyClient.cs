/*
 * (FluffyClient.cs)
 *------------------------------------------------------------
 * Created - 11/26/2025 8:49:01 PM
 * Created by - Seliris
 *-------------------------------------------------------------
 */

using System.Net.Sockets;

namespace FluffyByte.MUD.Driver.Core.Daemons.Networkd.Clients;

/// <summary>
/// The FluffyClient is essentially the wrapper around the underlying Tcp Socket (Socket) 
/// </summary>
public sealed class FluffyClient
{
    /// <summary>
    /// Contains the FluffyClient's underlying Tcp Socket.
    /// </summary>
    public Socket? Socket { get; private set; }

}

/*
*------------------------------------------------------------
* (FluffyClient.cs)
* See License.txt for licensing information.
*-----------------------------------------------------------
*/