/*
 * (FluffyClient.cs)
 *------------------------------------------------------------
 * Created - 11/26/2025 8:49:01 PM
 * Created by - Seliris
 *-------------------------------------------------------------
 */
using System.Net;
using System.Net.Sockets;
using FluffyByte.MUD.Driver.FluffyTools;

namespace FluffyByte.MUD.Driver.Core.Daemons.NetworkD.Clients;

/// <summary>
/// The FluffyClient is essentially the wrapper around the underlying Tcp Socket (Socket)
/// /// </summary>
public sealed class FluffyClient : IDisposable
{
    private readonly Socket? _socket;
    
    /// <summary>
    /// The name of the FluffyClient
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// The Guid for the FluffyClient.
    /// </summary>
    public Guid Guid { get; init; }

    /// <summary>
    /// The address associated with the FluffyClient instance.
    /// This property represents either the IP address or hostname
    /// used to identify the client on the network.
    /// </summary>
    public string Address { get; init; }

    /// <summary>
    /// The DNS address associated with the client. This property is initialized during object creation
    /// and is immutable thereafter.
    /// </summary>
    public string DnsAddress { get; init; }

    private CancellationTokenRegistration _shutdownRegistration;
    private bool _disconnecting;
    private bool _disposing;
    
    /// <summary>
    /// Represents a client that serves as a wrapper around a TCP socket, enabling communication and data exchange.
    /// </summary>
    public FluffyClient(Socket socket)
    {
        Guid = Guid.NewGuid();

        _socket = socket;

        if (socket.RemoteEndPoint is IPEndPoint ep)
        {
            Address = ep.Address.ToString();
            var dns = Dns.GetHostEntry(ep.Address);
            
            DnsAddress = dns.HostName.Length == 0 ? "unknown.com" : dns.HostName;
        }
        else
        {
            Address = "0.0.0.0";
            DnsAddress = "unknown.com";
            
            RequestDisconnect();
        }
        
        Name = $"Client({Address}/{DnsAddress})";
        
        _shutdownRegistration = SystemDaemon.GlobalShutdownToken.Register(ShutdownRequested);
        _disconnecting = false;
        _disposing = false;
    }


    /// <summary>
    /// Requests a disconnection of the client from its associated socket, ensuring the resources are released and
    /// no further communication occurs. If a disconnection has already been initiated, this method will
    /// take no action.</summary>
    public void RequestDisconnect(string reason = "No reason given")
    {
        if (_disconnecting)
            return;

        _disconnecting = true;
        Log.Info($"Disconnect for {Name} requested with reason: {reason}.");
        Dispose();
    }
    
    /// <summary>
    /// Disposes the FluffyClient
    /// </summary>
    public void Dispose()
    {
        if (_disposing)
            return;

        _disposing = true;
        
        _shutdownRegistration.Dispose();
        _socket?.Close();
        _socket?.Dispose();
    }

    private void ShutdownRequested()
    {
        RequestDisconnect("Shutdown was called.");
    }
}
/*
*------------------------------------------------------------
* (FluffyClient.cs)
* See License.txt for licensing information.
*-----------------------------------------------------------
*/
