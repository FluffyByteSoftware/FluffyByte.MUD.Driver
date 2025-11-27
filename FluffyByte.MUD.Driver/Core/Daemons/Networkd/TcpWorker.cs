/*
 * (TcpWorker.cs)
 *------------------------------------------------------------
 * Created - 11/26/2025 8:00:28 PM
 * Created by - Seliris
 *-------------------------------------------------------------
 */

using System.Net;
using System.Net.Sockets;
using FluffyByte.MUD.Driver.FluffyTools;

namespace FluffyByte.MUD.Driver.Core.Daemons.Networkd;

/// <summary>
/// Provides the actual socket and TCP handling for netowrk connections.
/// </summary>
public sealed class TcpWorker
{
    internal Socket _listener;
    private IPAddress _hostAddy = IPAddress.Parse(Constellations.HOST_ADDRESS);
    private int _hostPort = Constellations.HOST_PORT;

    
    private CancellationTokenSource _linkedCts;

    /// <summary>
    /// Initializes a new instance of the <see cref="TcpWorker"/> class.
    /// </summary>
    public TcpWorker()
    {
        _listener = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _linkedCts = CancellationTokenSource.CreateLinkedTokenSource(SystemDaemon.GlobalShutdownToken);
    }

    /// <summary>
    /// Starts the TCP listener and begins accepting connections.
    /// </summary>
    public static async ValueTask RequestStart()
    {
        try
        {

        }
        catch (OperationCanceledException)
        {
            Log.Debug($"RequestStart generated an exception but we assume its from a global shutdown.");
            return;
        }
        catch(Exception ex)
        {
            Log.Error($"TcpWorker RequestStart generated an exception.", ex);
        }
    }
}

/*
*------------------------------------------------------------
* (TcpWorker.cs)
* See License.txt for licensing information.
*-----------------------------------------------------------
*/