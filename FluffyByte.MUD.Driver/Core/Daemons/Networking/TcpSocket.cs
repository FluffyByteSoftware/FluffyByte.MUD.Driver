/*
 * (TcpSocket.cs)
 *------------------------------------------------------------
 * Created - 8:10:08 PM
 * Created by - Jacob Chacko
 *------------------------------------------------------------
 */

using System.Net;
using System.Net.Sockets;
using FluffyByte.MUD.Driver.FluffyTools;

namespace FluffyByte.MUD.Driver.Core.Daemons.Networking;

/// <summary>
/// Represents a TCP socket wrapper that provides controlled management of network sockets
/// and integrates with global system daemon events for graceful shutdown handling.
/// </summary>
/// <remarks>
/// The TcpSocket class is primarily used to manage TCP connections in a network environment.
/// It ensures proper cleanup of resources and facilitates system-wide shutdown procedures by
/// listening to global shutdown events emitted by the SystemDaemon. When a shutdown event occurs,
/// the socket gracefully stops accepting new connections and releases allocated resources.
/// </remarks>
/// <exception cref="ObjectDisposedException">
/// Thrown when attempting to use the TcpSocket object after it has been disposed.
/// </exception>
public sealed class TcpSocket : IDisposable
{
    private Socket _listeningSocket;
    private EndPoint _hostEndPoint;
    
    #region Life Cycle
    private CancellationTokenRegistration _shutdownRegistration;

    /// <summary>
    /// Represents a TCP socket wrapper that manages a network socket
    /// and facilitates controlled shutdown upon a global system shutdown event.
    /// </summary>
    /// <remarks>
    /// The TcpSocket class is designed to handle TCP connections in networked environments and
    /// integrates seamlessly with global system daemon events. It ensures that connections
    /// are gracefully closed when the application initiates a system-wide shutdown.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">
    /// Thrown if an attempt is made to use the TcpSocket instance after it has been disposed of.
    /// </exception>
    public TcpSocket()
    {
        _shutdownRegistration = new CancellationTokenRegistration();
        _shutdownRegistration = SystemDaemon.GlobalShutdownToken.Register(RequestShutdown);
        _listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, 
            ProtocolType.Tcp);
        _hostEndPoint = new IPEndPoint(IPAddress.Parse(Constellations.HostAddress), 
            Constellations.HostPort);
    }

    /// <summary>Initiates the startup process for the TCP socket by configuring and binding the socket to a
    /// specified endpoint, starting to listen for incoming connections, and registering a shutdown handler with
    /// the global system daemon.</summary>
    /// <remarks>This method sets up the TCP socket for accepting new connections by initializing its parameters,
    /// binding it to the configured host endpoint, and starting the listening process. It also integrates with
    /// the global shutdown mechanism to ensure a clean and graceful shutdown when a system-wide shutdown event
    /// is triggered.</remarks>
    /// <exception cref="ArgumentNullException">Thrown if the host address or port configuration is null or invalid.
    /// </exception>
    /// <exception cref="SocketException">Thrown if an error occurs while binding or listening on the
    /// socket.</exception>
    /// <exception cref="OperationCanceledException">Thrown if the operation is canceled, typically during a
    /// shutdown event.</exception>
    /// <exception cref="Exception">Thrown if an unexpected exception occurs during the initialization
    /// process.</exception>
    public void RequestStart()
    {
        try
        {
            Log.Debug($"TcpWorker: RequestStart() called.");
            _shutdownRegistration = new CancellationTokenRegistration();
            _shutdownRegistration = SystemDaemon.GlobalShutdownToken.Register(RequestShutdown);
            
            _listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, 
                ProtocolType.Tcp);
            _hostEndPoint = new IPEndPoint(IPAddress.Parse(Constellations.HostAddress), 
                Constellations.HostPort);
            
            _listeningSocket.Bind(_hostEndPoint);
            _listeningSocket.Listen(Constellations.MaximumNumberSockets);

            Log.Debug($"TcpWorker: Listening on {_hostEndPoint}.");
            Log.Debug($"TcpWorker: RequestStart() completed and listening.");
            
            Thread.Sleep(1000);
            _= ListenForConnections();
            
        }
        catch (OperationCanceledException)
        {
            // Expected during shutdown
            Log.Warn($"OperationCanceled during RequestStart of TcpSocket.");
        }
        catch (Exception ex)
        {
            Log.Error($"Exception during RequestStart of TcpSocket.", ex);
        }
    }

    /// <summary>Releases the resources used by the TcpSocket instance, including the underlying TCP listening socket
    /// and any active registrations for global shutdown events.</summary>
    /// <remarks>This method ensures the proper cleanup of resources associated with the TcpSocket instance. It
    /// disposes of the internal listening socket to release system-level resources and unregisters the shutdown
    /// cancellation token registration to prevent memory leaks.</remarks>
    /// <exception cref="ObjectDisposedException">Thrown if the underlying socket has already been disposed when this
    /// method is called. </exception>
    public void Dispose()
    {
        _listeningSocket.Close();
        
        _shutdownRegistration.Dispose();
        _listeningSocket.Dispose();
    }

    /// <summary>
    /// Initiates the shutdown process for the current TCP socket by closing and shutting down the associated
    /// listening socket. This method is automatically triggered when the global shutdown is called.</summary>
    /// <remarks>During the shutdown process, the method ensures that all active socket connections are
    /// terminated gracefully. It also logs warnings if an operation is canceled, which is expected
    /// behavior during the shutdown phase. Any unexpected exceptions encountered during the shutdown
    /// are logged for debugging purposes.</remarks>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled during the
    /// shutdown process. This is handled internally and logged as a warning.</exception>
    /// <exception cref="Exception">Captures and logs any unexpected exceptions that occur during
    /// the shutdown procedure.</exception>
    private void RequestShutdown()
    {
        try
        {
            _listeningSocket.Close();
            
            Dispose();
        }
        catch (OperationCanceledException)
        {
            // Expected during shutdown
            Log.Warn($"OperationCanceled during RequestShutdown of TcpSocket.");
        }
        catch (Exception ex)
        {
            Log.Error($"Exception during RequestShutdown of TcpSocket.", ex);
        }
        
    }
    #endregion Life Cycle
    
    #region Listening For Connections
    /// <summary>Continuously listens for incoming TCP client connections on the bound socket.</summary>
    /// <remarks>This method is an asynchronous task that remains active as long as the system is not undergoing
    /// a shutdown, and the associated switchboard is operational. It accepts incoming connections until the maximum
    /// number of allowable connections is reached, as defined by the system configuration. Each accepted
    /// client connection is handed off to a handler for further processing.</remarks>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled, typically as part
    /// of a graceful shutdown procedure.</exception>
    /// <exception cref="Exception">Thrown if an unexpected error occurs while listening for incoming client
    /// connections.</exception>
    /// <returns>A task that represents the asynchronous operation of listening for and accepting
    /// client connections.</returns>
    private async Task ListenForConnections()
    {
        try
        {
            _listeningSocket.Listen(Constellations.MaximumNumberSockets);
            Log.Debug($"TcpSocket: Listening for connections.");

            while (!SystemDaemon.GlobalShutdownToken.IsCancellationRequested && NetworkDaemon.Switchboard != null)
            {
                if (NetworkDaemon.Switchboard.ClientCount < Constellations.MaximumNumberSockets)
                {
                    var clientSocket = await _listeningSocket.AcceptAsync();
                    _ = HandleClientConnection(clientSocket);
                }
            }
        }
        catch (SocketException)
        {
            // Expected during shutdown
            Log.Debug($"SocketException during ListenForConnections of TcpSocket. This is expected!");
        }
        catch (OperationCanceledException)
        {
            // Expected during shutdown
            Log.Debug($"OperationCanceled during ListenForConnections of TcpSocket.");
        }
        catch (Exception ex)
        {
            Log.Error($"Exception during ListenForConnections of TcpSocket.", ex);
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Handles an individual client connection by initializing the client
    /// and managing its lifecycle while respecting global system shutdown requests.
    /// </summary>
    /// <param name="clientSocket">The socket representing the client's network connection.</param>
    /// <returns>A Task that represents the asynchronous operation of handling the client connection.</returns>
    /// <exception cref="OperationCanceledException"> Thrown when the global shutdown token is triggered during the
    /// connection handling process. </exception>
    /// <exception cref="Exception">Thrown if an unexpected error occurs while managing the
    /// client connection.</exception>
    private static async Task HandleClientConnection(Socket clientSocket)
    {
        if (NetworkDaemon.Switchboard == null)
        {
            Log.Error($"Switchboard was null during HandleClientConnection()");
            return;
        }
    
        try
        {
            if (SystemDaemon.GlobalShutdownToken.IsCancellationRequested)
            {
                Log.Warn($"OperationCanceled during HandleClientConnection of TcpSocket.");
                clientSocket.Close();
                return;
            }

            var client = new FluffyClient(clientSocket);

            if (!await client.InitializeClient())
            {
                Log.Warn($"Failed to initialize client: {client.Name}");
                client.RequestDisconnect("Failed to initialize client.");
                return;
            }
        
            // Add to the switchboard FIRST so it gets ticked
            NetworkDaemon.Switchboard.AddClient(client);
        
            // Queue messages AFTER it's in the switchboard
            client.QueueWrite("Welcome to FluffyByte MUD!\r\n");
            client.QueueWrite("Connection established.\r\n");
        }
        catch (OperationCanceledException)
        {
            // Expected during shutdown
            Log.Warn($"OperationCanceled during HandleClientConnection of TcpSocket.");
            if(clientSocket.Connected)
                clientSocket.Close();
        }
        catch (Exception ex)
        {
            Log.Error($"Exception during HandleClientConnection of TcpSocket.", ex);
            clientSocket.Close();
        }
    }
    #endregion
}
/*
 *------------------------------------------------------------
 * (TcpSocket.cs)
 * See License.txt for licensing information.
 *-----------------------------------------------------------
 */