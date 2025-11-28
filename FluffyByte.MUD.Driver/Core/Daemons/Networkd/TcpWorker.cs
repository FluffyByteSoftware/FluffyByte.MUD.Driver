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
    private Socket? _listener;
    
    private readonly IPAddress _hostAddress = IPAddress.Parse(Constellations.HostAddress);
    private readonly int _hostPort = Constellations.HostPort;
    private readonly CancellationTokenSource _linkedCts;

    /// <summary>
    /// Initializes a new instance of the <see cref="TcpWorker"/> class.
    /// </summary>
    public TcpWorker()
    {
        _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        
        _linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            SystemDaemon.GlobalShutdownToken
        );
    }

    /// <summary>
    /// Starts the TCP listener and begins accepting connections.
    /// </summary>
    public async ValueTask RequestStart()
    {
        
        try
        {
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            
            IPEndPoint temp = new(_hostAddress, _hostPort);
            
            _listener.Bind(temp);

            _listener.Listen(100);
            
            _ = ListenForConnectionLoop();

            await Task.CompletedTask;
        }
        catch (OperationCanceledException)
        {
            Log.Debug(
                $"RequestStart generated an exception but we assume its from a global shutdown."
            );
        }
        catch (Exception ex)
        {
            Log.Error($"TcpWorker RequestStart generated an exception.", ex);
        }
    }

    private async Task ListenForConnectionLoop()
    {
        while (!_linkedCts.IsCancellationRequested && _listener != null)
        {
            await Task.CompletedTask;
        }
    }
}

/*
*------------------------------------------------------------
* (TcpWorker.cs)
* See License.txt for licensing information.
*-----------------------------------------------------------
*/
