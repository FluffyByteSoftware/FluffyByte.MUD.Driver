/*
 * (FluffyClient.cs)
 *------------------------------------------------------------
 * Created - 11/26/2025 8:49:01 PM
 * Created by - Seliris
 *-------------------------------------------------------------
 */

using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using FluffyByte.MUD.Driver.FluffyTools;

namespace FluffyByte.MUD.Driver.Core.Daemons.NetworkD;

/// <summary>Represents a network client in the FluffyByte MUD system.</summary>
/// <remarks>The <see cref="FluffyClient"/> class provides methods and properties to manage a network client
/// within the FluffyByte MUD system. It handles the lifecycle of client connections, including initialization,
/// disconnection, and disposal. Each client is uniquely identified with a GUID.</remarks>
public sealed class FluffyClient : IDisposable
{
    #region Wrapped Variables
    private readonly Socket _socket;
    #endregion Wrapped Variables
    
    #region Buffers
    private readonly ConcurrentQueue<byte[]> _commandQueue = [];
    private readonly ConcurrentQueue<byte[]> _writerQ = [];
    #endregion Buffers
    
    #region Life Cycle
    private readonly CancellationTokenRegistration _shutdownRegistration;
    /// <summary>
    /// Gets a unique identifier (GUID) for an instance of the <see cref="FluffyClient"/> class.
    /// </summary>
    /// <remarks>
    /// This property provides a globally unique identifier for a client within the FluffyByte system.
    /// It ensures that each instance of the client can be uniquely identified across the system, enabling
    /// proper tracking, management, and interaction within the <see cref="Switchboard"/> and other
    /// components.
    /// </remarks>
    public Guid Guid { get; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the name of the network client.
    /// </summary>
    /// <remarks>
    /// This property specifies the name assigned to a client within the <see cref="FluffyClient"/> instance.
    /// It is primarily used for logging, identification, and debugging purposes across the FluffyByte MUD system.
    /// The name is set during the client's initialization and can be updated if necessary to reflect meaningful
    /// or user-defined identifiers.
    /// </remarks>
    public string Name { get; private set; }

    /// <summary>
    /// Gets or sets the network address associated with the instance of the <see cref="FluffyClient"/> class.
    /// </summary>
    /// <remarks>
    /// This property represents the network address (e.g., IP address or hostname) used to connect and communicate
    /// with the client in the FluffyByte MUD System. It is essential for identifying the client's network endpoint
    /// and facilitating data exchange within the system.</remarks>
    private readonly string _address;

    /// <summary>Gets the IP address associated with the client.</summary>
    private readonly IPAddress? _ipAddress;

    /// <summary>Gets the DNS address associated with the client.</summary>
    private string? DnsAddress { get; set; }

    private bool _disconnecting;
    private bool _disposing;

    /// <summary>Represents a network client in the FluffyByte MUD application. This class provides
    /// functionality for managing client connections: including initialization, communication,
    /// and disconnection activities.</summary>
    /// <remarks>FluffyClient is designed to manage the lifecycle of a single client connection
    /// and integrates with the broader system daemons for handling global shutdowns. </remarks>
    public FluffyClient(Socket socket)
    {
        _socket = socket;
        _disconnecting = false;
        _disposing = false;

        Name = $"FluffyClient-{Guid}";
        
        if(_socket.RemoteEndPoint is not IPEndPoint ep)
            throw new InvalidOperationException("Socket does not have an IP endpoint.");

        _ipAddress = ep.Address;
        _address = _ipAddress.ToString();
        DnsAddress = "unresolved.com";
        
        _shutdownRegistration = new CancellationTokenRegistration();
        _shutdownRegistration = SystemDaemon.GlobalShutdownToken.Register(RequestShutdown);
        Log.Info($"Client has joined.");
    }

    /// <summary>Initializes the FluffyClient instance by resolving its DNS address, generating a unique name,
    /// and setting up necessary metadata for client identification and communication.</summary>
    /// <remarks>This method attempts to resolve the client's DNS address using the provided IP address.
    /// It generates a unique client name based on the GUID and address details, which gets used for
    /// logging and client identification within the system.</remarks>
    /// <returns>A task representing the asynchronous operation. The task result is a boolean value:
    /// true if the client was successfully initialized; false if an error or cancellation occurred
    /// during initialization.</returns>
    public async Task<bool> InitializeClient()
    {
        if (_ipAddress is null)
        {
            RequestDisconnect();
            return false;
        }

        try
        {
            var hostEntry = await Dns.GetHostAddressesAsync(_address);
            DnsAddress = hostEntry.Length > 0 ? hostEntry[0].ToString() : "unresolved.com";

            Name = $"{Guid}_{_address}/({DnsAddress})";
            Log.Debug($"Created FluffyClient: {Name}");

            return true;
        }
        catch (OperationCanceledException)
        {
            // Expected during a shutdown
            Log.Debug($"Operation canceled during InitializeClient");
            return false;
        }
        catch (Exception ex)
        {
            Log.Error($"Exception during InitializeClient.", ex);
            return false;
        }
    }

    /// <summary>Requests a disconnection for the current client and performs the necessary cleanup steps.</summary>
    /// <param name="reason">An optional reason describing why the client is being disconnected.
    /// Defaults to "No reason given" if not specified.</param>
    /// <remarks>This method ensures that the client's connection is gracefully terminated by closing the
    /// underlying socket, logging the disconnection, and informing the network management
    /// components. It also handles exceptions that may occur during the disconnection process
    /// and enforces proper disposal of the client instance.</remarks>
    public void RequestDisconnect(string reason = "No, reason given")
    {
        if (_disconnecting || _disposing)
            return;

        try
        {
            _disconnecting = true;

            _socket.Close();

            Log.Debug($"Disconnected FluffyClient: {Name} ({reason})");

            if (NetworkDaemon.Switchboard is not null)
                NetworkDaemon.Switchboard.RemoveClient(this);
        }
        catch (OperationCanceledException)
        {
            // Expected during a shutdown
            Log.Debug($"Operation canceled during RequestDisconnect");
        }
        catch (Exception ex)
        {
            Log.Error($"Exception during RequestDisconnect.", ex);
            _socket.Close();
        }
        
        Dispose();
    }
    
    /// <summary>Releases all resources used by the current instance of the
    /// <see cref="FluffyClient"/> class.</summary>
    /// <remarks>This method ensures proper cleanup of client resources, including closing the underlying socket,
    /// unregistering from global shutdown notifications, and marking the client as disposed. If the client
    /// has not been disconnected at the time of disposal, it will automatically request disconnection
    /// before proceeding with resource cleanup.</remarks>
    public void Dispose()
    {
        if (!_disconnecting)
        {
            RequestDisconnect("Client disposed without disconnecting.");
            return;
        }

        if (_disposing)
            return;
        
        _socket.Close();
        _socket.Dispose();
        _shutdownRegistration.Dispose();
        
        _disposing = true;
    }

    private string _pendingPrompt = "> ";

    /// <summary>Updates the client's input prompt to a specified string. This prompt will be displayed to the client
    /// when they are awaiting input.</summary>
    /// <param name="prompt">The new prompt string to be set. This value will replace the current prompt.</param>
    public void SetPrompt(string prompt) => _pendingPrompt = prompt;

    /// <summary>Processes the client's main operational logic, including sending queued data and handling errors
    /// during communication. This method is designed to be called periodically to maintain the state and
    /// functionality of the client connection.</summary>
    /// <remarks>ClientTick handles any pending output by flushing the writer queue to the client and catching
    /// exceptions that might occur during the operation. It ensures graceful handling of cancellation
    /// scenarios (e.g., during shutdown) or unexpected errors, logging them and triggering disconnection
    /// if necessary.</remarks>
    /// <returns>A <see cref="ValueTask"/> that represents the asynchronous operation.</returns>
    public async ValueTask ClientTick()
    {
        if (_disposing || _disconnecting)
            return;

        try
        {
            // Flush all pending output to the client
            while (_writerQ.TryDequeue(out var buffer))
            {
                Log.Info($"_writerQ: {Encoding.UTF8.GetString(buffer)}");
                await SendBytesAsync(buffer);
            }

            // Queue the prompt for next sending
            if (!string.IsNullOrEmpty(_pendingPrompt))
            {
                QueueWrite(_pendingPrompt);
                _pendingPrompt = ""; // Clear it so it only sends once
            }
        }
        catch (SocketException)
        {
            // Expected during a disconnect or shutdown
            Log.Debug($"SocketException during ClientTick on {Name}");
            RequestDisconnect("Likely a dropped connection.");
        }
        catch (OperationCanceledException)
        {
            // Expected during a shutdown
            Log.Debug($"Operation canceled during ClientTick on {Name}");
        }
        catch (Exception ex)
        {
            Log.Error($"Exception during ClientTick on {Name}", ex);
            RequestDisconnect();
        }
    }
    
    private void RequestShutdown() => RequestDisconnect("A server shutdown was requested.");
    #endregion Lifecycle
    
    #region Input Output
    private readonly List<byte> _inputBuffer = [];
    private readonly Lock _inputLock = new();

    private const int MAX_LINE_LENGTH = 2048;

    /// <summary>Enqueues a byte array into the client's writer queue for asynchronous sending.</summary>
    /// <param name="buffer">The byte array to be enqueued. Must not be empty.</param>
    public void QueueWrite(byte[] buffer)
    {
        if (buffer.Length == 0)
        {
            Log.Debug($"Attempted to enqueue empty buffer to {Name}.");
            return;
        }

        _writerQ.Enqueue(buffer);
    }

    /// <summary>Enqueues a UTF-8 encoded string into the client's writer queue for asynchronous sending.</summary>
    /// <param name="text">The string to be enqueued. Must not be null or empty.</param>
    public void QueueWrite(string text) => QueueWrite(Encoding.UTF8.GetBytes(text));

    /// <summary>Asynchronously sends a sequence of bytes to the associated network client.</summary>
    /// <param name="buffer">The byte array containing the data to be sent.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation, with a result indicating
    /// whether the send operation completed successfully.</returns>
    private async ValueTask SendBytesAsync(byte[] buffer)
    {
        try
        {
            var totalSent = 0;

            while (totalSent < buffer.Length)
            {
                var sent = await _socket.SendAsync(
                    buffer.AsMemory(totalSent),
                    SocketFlags.None);

                if (sent == 0)
                {
                    throw new SocketException();
                }

                totalSent += sent;
            }

            Log.Debug($"Sending {buffer.Length} bytes to {Name}");
        }
        catch (OperationCanceledException)
        {
            // Expected during a shutdown
            Log.Debug($"Operation canceled during SendBytesAsync");
            RequestDisconnect("Likely a shutdown.");
        }
        catch (SocketException)
        {
            Log.Debug($"SocketException during SendBytesAsync.");
            RequestDisconnect("likely a dropped connection.");
        }
        catch (Exception ex)
        {
            Log.Error($"Exception during SendBytesAsync.", ex);
        }
    }

    /// <summary>Asynchronously reads data from the client's socket into an internal buffer.</summary>
    /// <remarks>This method continuously reads bytes from the network socket until the connection is
    /// terminated or an error occurs. The received data is stored in an internal buffer and
    /// processed to extract complete lines based on predefined rules. If the input exceeds
    /// the maximum allowed length or the connection is interrupted, the client is disconnected.</remarks>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation, which completes
    /// when the data is successfully read or if the connection gets terminated.</returns>
    public async ValueTask ReadBytesAsync()
    {
        if (!_socket.Connected || _disposing || _disconnecting)
        {
            Log.Debug($"Socket not connected or disposed {Name}");
            RequestDisconnect("Socket is not connected or client is disposed.");
            return;
        }

        try
        {
            var buffer = new byte[512];
            var bytesRead = await _socket.ReceiveAsync(buffer, SocketFlags.None);

            if (bytesRead == 0)
            {
                RequestDisconnect("Socket disconnected by remote host.");
                return;
            }

            lock (_inputLock)
            {
                _inputBuffer.AddRange(buffer[..bytesRead]);

                if (_inputBuffer.Count > MAX_LINE_LENGTH)
                {
                    RequestDisconnect($"Input line exceeds maximum length of {MAX_LINE_LENGTH} bytes.");
                    return;
                }

                ProcessCompleteLines();
            }
        }
        catch (OperationCanceledException)
        {
            // Expected during a shutdown
            Log.Debug($"Operation canceled during ReadBytesAsync");
            RequestDisconnect("Likely a shutdown.");
        }
        catch (SocketException)
        {
            // Expected during a disconnect or shutdown
            Log.Debug($"SocketException during ReadBytesAsync");
        }
        catch (Exception ex)
        {
            Log.Error("Exception during ReadBytesAsync", ex);
            RequestDisconnect();
        }
    }
    
    private void ProcessCompleteLines()
    {
        while (true)
        {
            int lineEndIndex = -1;
            int terminatorLength = 0;

            for (int i = 0; i < _inputBuffer.Count; i++)
            {
                if (_inputBuffer[i] == '\r')
                {
                    // Check for \r\n
                    if (i + 1 < _inputBuffer.Count && _inputBuffer[i + 1] == '\n')
                    {
                        lineEndIndex = i;
                        terminatorLength = 2;
                        break;
                    }
                    
                    // standalone \r
                    lineEndIndex = i;
                    terminatorLength = 1;
                    break;
                }
                else if (_inputBuffer[i] == '\n')
                {
                    lineEndIndex = i;
                    terminatorLength = 1;
                    break;
                }
            }
            
            // No complete line yet
            if (lineEndIndex == -1)
                break;

            // Extract the command (without the terminator)
            var lineBytes = _inputBuffer.Take(lineEndIndex).ToArray();
            var command = Encoding.UTF8.GetString(lineBytes).TrimEnd();
            
            if(!string.IsNullOrEmpty(command))
                _commandQueue.Enqueue(Encoding.UTF8.GetBytes(command));

            _inputBuffer.RemoveRange(0, lineEndIndex + terminatorLength);
        }
    }

    /// <summary>Retrieves and removes the next command from the client's command queue, if available.</summary>
    /// <remarks>This method is thread-safe and ensures that commands are dequeued in the order they were received.
    /// It is intended for use in scenarios where commands need to be processed sequentially.</remarks>
    /// <returns>The next command from the queue as a byte array if one is available; otherwise, null.</returns>
    public byte[]? DequeueCommand()
    {
        lock (_inputLock)
        {
            _commandQueue.TryDequeue(out var command);
            return command;
        }
    }
    #endregion Input Output
}
/*
*------------------------------------------------------------
* (FluffyClient.cs)
* See License.txt for licensing information.
*-----------------------------------------------------------
*/
