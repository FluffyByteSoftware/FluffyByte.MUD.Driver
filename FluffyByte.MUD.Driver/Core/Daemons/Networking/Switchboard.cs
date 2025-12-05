/*
 * (Switchboard.cs)
 *------------------------------------------------------------
 * Created - 5:00:11 PM
 * Created by - Jacob Chacko
 *------------------------------------------------------------
 */

using System.Collections.Concurrent;
using System.Net.Sockets;
using FluffyByte.MUD.Driver.FluffyTools;

namespace FluffyByte.MUD.Driver.Core.Daemons.Networking;

/// <summary>The Switchboard class is responsible for managing a collection of connected clients within
/// the networking subsystem. It maintains a mapping of unique client identifiers (GUIDs) to their respective
/// FluffyClient objects, facilitating efficient client management and interaction.</summary>
/// <remarks>The Switchboard serves as a central hub for tracking connected clients in a concurrent and
/// thread-safe manner, leveraging a ConcurrentDictionary to store client connections. This design
/// ensures scalability and consistency across network operations in a multithreaded environment.</remarks>
public sealed class Switchboard : IDisposable
{
    private readonly Lock _localLock = new();

    #region Client Management
    /// <summary>Gets or internally sets the collection of currently connected clients in the Switchboard.</summary>
    /// <remarks>This property represents a thread-safe dictionary that maps unique client identifiers (GUIDs)
    /// to their corresponding <see cref="FluffyClient"/> instances. It is used to track and manage
    /// active connections in a scalable and consistent manner within the networking subsystem.
    /// The dictionary allows efficient lookup, addition, and removal of client connections.</remarks>
    private ConcurrentDictionary<Guid, FluffyClient> ConnectedClients { get; set; } = [];
    
    /// <summary>Adds a new client to the collection of connected clients managed by the Switchboard.</summary>
    /// <param name="clientToAdd">The FluffyClient instance to be added to the Switchboard's collection.</param>
    /// <remarks>If the specified client is already in the collection, the method logs a warning and exits
    /// without adding the duplicate client. This method ensures thread-safe operations by locking
    /// during the addition process to maintain consistency in the ConnectedClients collection.</remarks>
    public void AddClient(FluffyClient clientToAdd)
    {
        if (ConnectedClients.Values.Contains(clientToAdd))
        {
            Log.Warn($"Switchboard asked to add a client that's already in its list.");
            return;
        }

        lock (_localLock)
        {
            Log.Debug($"Adding client with GUID: {clientToAdd.Guid}, {clientToAdd.Name} to Switchboard.");
            ConnectedClients.TryAdd(clientToAdd.Guid, clientToAdd);
        }
    }

    /// <summary>Removes a specified client from the collection of connected clients managed
    /// by the Switchboard.</summary>
    /// <param name="clientToRemove">The FluffyClient instance to be removed from the Switchboard's collection.</param>
    /// <remarks>If the specified client is not found in the collection, the method logs a warning and exits.
    /// This method uses a thread-safe approach by locking during the removal process to ensure consistency in the
    /// ConnectedClients collection.</remarks>
    public void RemoveClient(FluffyClient clientToRemove)
    {
        if (!ConnectedClients.Values.Contains(clientToRemove))
        {
            Log.Warn($"Switchboard asked to remove a client that's not found in the list.");
            return;
        }

        lock (_localLock)
        {
            Log.Debug($"Removing client with GUID: {clientToRemove.Guid}, {clientToRemove.Name} from Switchboard.");
            ConnectedClients.TryRemove(clientToRemove.Guid, out _);
        }
    }

    /// <summary>Gets the total number of currently connected clients managed by the Switchboard.</summary>
    /// <remarks>This property provides a thread-safe count of the active connections stored in the Switchboard's
    /// collection. It reflects the current state of connected clients, enabling the networking subsystem
    /// to monitor and manage the system's load or allocate resources accordingly.</remarks>
    public int ClientCount => ConnectedClients.Count;
    
    /// <summary>Retrieves the FluffyClient instance associated with the given unique identifier (GUID).</summary>
    /// <param name="guid">The unique identifier (GUID) of the client to be retrieved from the collection.</param>
    /// <returns>The FluffyClient instance corresponding to the specified GUID, or null if no client with
    /// the provided GUID exists in the collection.</returns>
    /// <remarks>This method is used to look up and return a client based on its unique identifier.
    /// The operation is thread-safe, as the underlying ConnectedClients collection is a
    /// ConcurrentDictionary.</remarks>
    public FluffyClient GetClient(Guid guid) => ConnectedClients[guid];

    /// <summary>
    /// Retrieves a list containing all currently connected clients managed by the Switchboard.
    /// </summary>
    /// <remarks>This property provides a snapshot of the connected clients by extracting the values
    /// from the thread-safe dictionary used internally by the Switchboard. It returns a new
    /// <see cref="List{T}"/> of <see cref="FluffyClient"/> instances, representing the
    /// current state of all active client connections.
    /// It is particularly useful for iterating over the collection of connected clients in
    /// scenarios such as broadcasting messages or performing bulk operations.</remarks>
    public List<FluffyClient> AllClients => ConnectedClients.Values.ToList();

    /// <summary>
    /// Executes a tick operation on all connected clients managed by the Switchboard.
    /// </summary>
    /// <remarks>
    /// This method iterates through all connected clients and invokes their respective asynchronous ClientTick method.
    /// It is typically used to perform periodic or scheduled tasks that need to be executed for each active client.
    /// Logs a debug message at the start of the operation to indicate its execution.
    /// </remarks>
    /// <returns>A ValueTask representing the asynchronous operation of ticking all clients.</returns>
    public async ValueTask TickAllClients()
    {
        foreach (var client in AllClients)
        {
            try
            {
                await client.ReadBytesAsync();
                await client.ClientTick();
            }
            catch (SocketException)
            {
                // Remote closed connection or invalid socket state.
                client.RequestDisconnect("SocketException during tick.");
                RemoveClient(client);
            }
            catch (Exception ex)
            {
                Log.Error($"Unexpected error in TickAllClients()", ex);
                client.RequestDisconnect("General exception in tick");
                RemoveClient(client);
            }
        }
    }
    
    #endregion Client Management
    
    #region Life Cycle
    private readonly CancellationTokenRegistration _shutdownRegistration;
    
    private static bool _disposing;

    /// <summary>Represents the central hub for managing a collection of connected clients
    /// within the FluffyByte MUD networking subsystem.</summary>
    /// <remarks>The Switchboard class serves as a thread-safe mechanism for handling client
    /// connections, using a ConcurrentDictionary for efficient storage and retrieval.
    /// It provides functionality for adding, removing, and querying client connections
    /// while ensuring consistency in a multithreaded environment.</remarks>
    public Switchboard()
    {
        _shutdownRegistration = new CancellationTokenRegistration();
        _shutdownRegistration = SystemDaemon.GlobalShutdownToken.Register(Shutdown);
    }
    
    /// <summary>Shuts down the Switchboard by disconnecting all connected clients and clearing
    /// internal resources.</summary>
    /// <remarks>This method iterates through all connected clients within the Switchboard and requests
    /// each client to disconnecting them with a specified reason. A debug log is recorded for each client
    /// disconnection, including its unique GUID and name. This operation is typically invoked
    /// during application termination or when the networking subsystem needs to be gracefully stopped.</remarks>
    private void Shutdown()
    {
        if (_disposing) return;
        
        foreach (var client in ConnectedClients.Values)
        {
            try
            {
                //client.FinalFlush();
                
                client.RequestDisconnect("Switchboard is shutting down.");
                Log.Debug($"Disconnecting client with GUID: {client.Guid}, {client.Name} from Switchboard.");
            }
            catch (Exception ex)
            {
                Log.Error($"Exception during Shutdown of Switchboard.", ex);
            }
        }
        
        ConnectedClients.Clear();
    }

    /// <summary>Releases resources used by the Switchboard instance and performs necessary cleanup
    /// operations.</summary>
    /// <remarks>This method ensures proper disposal of the Switchboard's resources, preventing potential
    /// memory leaks or resource contention. It unregisters the global shutdown token to avoid executing
    /// Shutdown multiple times and ensures thread-safe disposal operations by maintaining a flag to track
    /// if disposal has already occurred.</remarks>
    public void Dispose()
    {
        if (_disposing) return;

        _disposing = true;
        _shutdownRegistration.Dispose();
    }

    /// <summary>Gets a string representation of the current status of client connections.</summary>
    /// <remarks>This property provides an overview of the number of connected clients managed by the
    /// Switchboard. It returns a human-readable summary that includes the total count of active client connections.
    /// Primarily used for logging or monitoring purposes to track the state of the networking subsystem.</remarks>
    public string RequestStatus => $"Connected Clients: {ClientCount}";
    #endregion Life Cycle
}
/*
 *------------------------------------------------------------
 * (Switchboard.cs)
 * See License.txt for licensing information.
 *-----------------------------------------------------------
 */