/*
 * (FluffyClient.cs)
 *------------------------------------------------------------
 * Created - 11/26/2025 8:49:01 PM
 * Created by - Seliris
 *-------------------------------------------------------------
 */

using System.Net.Sockets;
using System.Text;
using FluffyByte.MUD.Driver.Core.Types.Daemons.Networking;
using FluffyByte.MUD.Driver.FluffyTools;

namespace FluffyByte.MUD.Driver.Core.Daemons.Networkd.Clients;

/// <summary>
/// The FluffyClient is essentially the wrapper around the underlying Tcp Socket (Socket)
/// /// </summary>
public sealed class FluffyClient
{
    private Socket? _socket;
    private readonly StringBuilder _lineBuffer;
    private readonly byte[] _receiveBuffer;

    private bool _disposed;
    private DateTime _lastActivityUtc;

    /// <summary>
    /// Gets the unique identifier for this FluffyClient instance.
    /// </summary>
    /// <returns>A new Guid representing the unique identifier.</returns>
    public Guid Guid { get; } = Guid.NewGuid();

    /// <summary>
    /// Size of the internal receive buffer in bytes.
    /// </summary>
    private const int RECEIVE_BUFFER_SIZE = 4096;

    /// <summary>
    /// Maximum number of input commands to queue per tick.
    /// Commands beyond this limit are discarded to prevent flooding.
    /// </summary>
    public const int MaxInputCommandsPerTick = 3;

    /// <summary>
    /// Soft limit on output messages queued for this client.
    /// The Switchboard uses this to decide when to warn or throttle.
    /// </summary>
    public const int SoftOutputLimit = 100;

    /// <summary>
    /// Maximum size of the line buffer in characters.
    /// Prevents memory abuse from misbehaving clients.
    /// </summary>
    private const int MAX_LINE_BUFFER_SIZE = 8192;

    /// <summary>
    /// Contains the FluffyClient's underlying Tcp Socket.
    /// </summary>
    public Socket? Socket => _socket;

    /// <summary>
    /// Gets the display name of the client instance.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the network address associated with this instance.
    /// </summary>
    public string Address { get; private set; }

    /// <summary>
    /// Stores the Dns address of the client as a string.
    /// </summary>
    public string DnsAddress { get; private set; }

    /// <summary>
    /// Gets or sets the current connection state of the client.
    /// </summary>
    /// <remarks>Use this property to determine whether the client is connecting, connected, disconnected, or
    /// in another defined state. The value reflects the client's most recent connection status and may change as
    /// connection attempts succeed or fail.</remarks>
    public ClientState State { get; set; }

    /// <summary>
    /// Gets the date and time, in Coordinated Universal Time (UTC), when the connection was established.
    /// </summary>
    public DateTime ConnectedAtUtc { get; private set; }

    /// <summary>
    /// Gets the date and time, in Coordinated Universal Time (UTC), when the last activity occurred.
    /// </summary>
    public DateTime LastActivityUtc => _lastActivityUtc;

    /// <summary>
    /// Gets a value indicating whether the socket connection is currently active and available for communication.
    /// </summary>
    /// <remarks>The value is <see langword="true"/> only if the socket has not been disposed, is initialized,
    /// and is in a connected and responsive state. This property does not guarantee that the connection will remain
    /// available; network conditions may change after the value is retrieved.</remarks>
    public bool IsConnected =>
        !_disposed && _socket != null && _socket.Connected && IsSocketAlive();

    /// <summary>
    /// Initializes a new instance of the FluffyClient class using the specified socket and network addresses.
    /// </summary>
    /// <param name="socket">The socket to use for client communication. If null, a NullReferenceException is
    /// thrown.</param>
    /// <param name="address">The network address associated with the client. This value is used for identification
    /// and connection purposes.</param>
    /// <param name="dnsAddress">The DNS address associated with the client. This value is used for identification
    /// and connection purposes.</param>
    /// <exception cref="NullReferenceException">Thrown if <paramref name="socket"/> is null.</exception>
    public FluffyClient(Socket? socket, string address, string dnsAddress)
    {
        _socket = socket ?? throw new NullReferenceException(nameof(socket));

        Address = address;
        DnsAddress = dnsAddress;
        Name = $"Client ({Address} | {DnsAddress})";

        ConnectedAtUtc = DateTime.UtcNow;
        _lastActivityUtc = DateTime.UtcNow;

        _lineBuffer = new();
        _receiveBuffer = new byte[RECEIVE_BUFFER_SIZE];

        // Configure socket for MUD-appropriate behavior

        State = ClientState.Connecting;
        _socket.NoDelay = true; // Disable Nagle's algorithm for responsiveness
        _socket.ReceiveTimeout = 0; // Non-blocking reads
        _socket.SendTimeout = 5000; // 5 second send timeout
    }

    /// <summary>
    /// Reads a line of text from the underlying buffer, using a line terminator to determine the end of the line.
    /// </summary>
    /// <remarks>Line terminators recognized are '\n' and '\r'. If a line terminator is immediately followed
    /// by another terminator of a different type (e.g., '\r\n' or '\n\r'), both are consumed. The returned line does
    /// not include the line terminator characters. Subsequent calls will return the next available line, if
    /// present.</remarks>
    /// <returns>A string containing the next line of text, with leading and trailing whitespace removed; or <see
    /// langword="null"/> if no complete line is available.</returns>
    public string? ReadLine()
    {
        ThrowIfDisposed();
        DrainSocket();

        string bufferContent = _lineBuffer.ToString();
        var newlineIndex = bufferContent.IndexOfAny(['\n', '\r']);

        if (newlineIndex < 0)
            return null;

        var line = bufferContent[..newlineIndex];

        // Remove the line and any trailing \r\n or \n\r combinations
        var endIndex = newlineIndex + 1;
        if (endIndex < bufferContent.Length)
        {
            var nextChar = bufferContent[endIndex];
            if ((nextChar == '\n' || nextChar == '\r') && nextChar != bufferContent[newlineIndex])
                endIndex++;
        }

        _lineBuffer.Remove(0, endIndex);

        // Clean the line: trim whitespace, handle telnet sequences later
        return line.Trim();
    }

    /// <summary>
    /// Sends the specified message to the connected remote endpoint using the underlying socket.
    /// </summary>
    /// <remarks>If the client is not connected, the method does not send the message and updates the
    /// connection state to disconnected. If an error occurs during transmission, the connection state is also set to
    /// disconnected. This method does not throw exceptions for transmission errors; instead, it updates the connection
    /// state accordingly.</remarks>
    /// <param name="message">The message to send. If <paramref name="message"/> is null or empty, no data is sent.</param>
    public void Write(string message)
    {
        ThrowIfDisposed();

        if (string.IsNullOrEmpty(message))
            return;

        if (!IsConnected || _socket == null)
        {
            State = ClientState.Disconnected;
            return;
        }

        try
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            var sent = 0;

            while (sent < bytes.Length)
            {
                sent += _socket.Send(bytes, sent, bytes.Length - sent, SocketFlags.None);
            }
        }
        catch (SocketException)
        {
            State = ClientState.Disconnected;
        }
        catch (ObjectDisposedException)
        {
            State = ClientState.Disconnected;
        }
        catch (Exception ex)
        {
            Log.Error(ex);
            State = ClientState.Disconnected;
        }
    }

    /// <summary>
    /// Determines whether the connection is actively polling based on recent activity.
    /// </summary>
    /// <remarks>If the connection is not established, this method returns false regardless of activity. Use
    /// this method to check whether the connection should be considered actively polling or idle.</remarks>
    /// <param name="idleThreshold">The maximum duration of inactivity, as a <see cref="TimeSpan"/>, before the connection is considered idle. If
    /// <see langword="null"/>, a default threshold of 5 minutes is used.</param>
    /// <returns>true if the connection has been active within the specified idle threshold; otherwise, false.</returns>
    public bool IsPolling(TimeSpan? idleThreshold = null)
    {
        ThrowIfDisposed();

        if (!IsConnected)
            return false;

        var threshold = idleThreshold ?? TimeSpan.FromMinutes(5);
        var idleTime = DateTime.UtcNow - _lastActivityUtc;

        return idleTime < threshold;
    }

    /// <summary>
    /// Reads and discards all available data from the underlying socket, updating the internal buffer and connection
    /// state as necessary.
    /// </summary>
    /// <remarks>This method processes incoming data by filtering out Telnet command sequences and appending
    /// the remaining text to the internal line buffer. If the buffer exceeds its maximum allowed size, older data is
    /// removed to prevent excessive memory usage. If the socket is disconnected or an error occurs during reading, the
    /// client state is set to disconnected. This method should be called only when the socket is connected and
    /// available for reading.</remarks>
    private void DrainSocket()
    {
        if (!IsConnected || _socket == null)
            return;

        try
        {
            while (_socket.Available > 0)
            {
                var bytesRead = _socket.Receive(
                    _receiveBuffer,
                    0,
                    _receiveBuffer.Length,
                    SocketFlags.None
                );

                if (bytesRead == 0)
                {
                    State = ClientState.Disconnected;
                    return;
                }

                _lastActivityUtc = DateTime.UtcNow;

                // Filter out telnet IAC sequences before adding to buffer
                var filteredBytes = FilterTelnetSequences(_receiveBuffer, bytesRead);
                var text = Encoding.UTF8.GetString(filteredBytes);
                _lineBuffer.Append(text);

                // Enforce buffer size limit to prevent memory abuse
                if (_lineBuffer.Length > MAX_LINE_BUFFER_SIZE)
                {
                    var excess = _lineBuffer.Length - MAX_LINE_BUFFER_SIZE;
                    _lineBuffer.Remove(0, excess);
                }
            }
        }
        catch (SocketException)
        {
            State = ClientState.Disconnected;
        }
        catch (Exception ex)
        {
            State = ClientState.Disconnected;
            Log.Error(ex);
        }
    }

    /// <summary>
    /// Filters out escape codes, and other artifacts from raw telnet data.
    /// </summary>
    /// <param name="buffer">The raw telnet data buffer.</param>
    /// <param name="length">The length of the data to filter.</param>
    /// <returns>A new byte array containing the filtered data.</returns>
    private static byte[] FilterTelnetSequences(byte[] buffer, int length)
    {
        const byte iac = 255;
        const byte se = 240; // Subnegotiation End
        const byte sb = 250; // Subnegotiation Begin

        var result = new byte[length];
        var resultIndex = 0;
        var i = 0;

        while (i < length)
        {
            if (buffer[i] == iac && i + 1 < length)
            {
                var command = buffer[i + 1];

                if (command == iac)
                {
                    result[resultIndex++] = iac;
                    i += 2;
                }
                else if (command == sb)
                {
                    // Subnegotiation: skip until IAC SE
                    i += 2;
                    while (i + 1 < length && !(buffer[i] == iac && buffer[i + 1] == se))
                    {
                        i++;
                    }

                    i += 2; // Skip the IAC SE
                }
                else if (command is >= 251 and <= 254)
                {
                    // WILL (251), WONT (252), DO (253), DONT (254)
                    // These are 3-byte sequences: IAC + command + option
                    i += 3;
                }
                else
                {
                    // Other 2-byte commands: IAC + command
                    i += 2;
                }
            }
            else
            {
                result[resultIndex++] = buffer[i];
                i++;
            }
        }

        // Return trimmed array
        var trimmed = new byte[resultIndex];
        Array.Copy(result, trimmed, resultIndex);

        return trimmed;
    }

    /// <summary>
    /// Checks if the socket is still alive using a poll operation.
    /// </summary>
    /// <returns>Returns true if the Socket is Alive</returns>
    private bool IsSocketAlive()
    {
        if (_socket == null)
            return false;

        try
        {
            // Poll returns true if: connection closed, data available, or error
            // if it returns true but no data is available, connection is dead
            if (_socket.Poll(0, SelectMode.SelectRead))
            {
                return _socket.Available > 0;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Throws an exception if the current instance has been disposed.
    /// </summary>
    /// <remarks>Call this method before performing operations that require the instance to be active. This
    /// helps prevent usage of an object after it has been disposed, which can lead to undefined behavior.</remarks>
    /// <exception cref="ObjectDisposedException">Thrown when the instance has already been disposed.</exception>
    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(FluffyClient));
    }

    /// <summary>
    /// Releases all resources used by the client and disconnects from the remote endpoint.
    /// </summary>
    /// <remarks>After calling this method, the client transitions to the disconnected state and cannot be
    /// used for further communication. Multiple calls to this method have no effect.</remarks>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        State = ClientState.Disconnected;

        if (_socket == null) return;
        
        try
        {
            if (_socket.Connected)
            {
                _socket.Shutdown(SocketShutdown.Both);
            }
        }
        catch (SocketException)
        {
            // Ignore shutdown errors
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
        finally
        {
            _socket.Dispose();
            _socket = null;
        }
    }
}
/*
*------------------------------------------------------------
* (FluffyClient.cs)
* See License.txt for licensing information.
*-----------------------------------------------------------
*/
