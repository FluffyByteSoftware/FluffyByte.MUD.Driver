/*
 * (Switchboard.cs)
 *------------------------------------------------------------
 * Created - 5:00:11 PM
 * Created by - Jacob Chacko
 *------------------------------------------------------------
 */

using System.Collections.Concurrent;
using FluffyByte.MUD.Driver.FluffyTools;

namespace FluffyByte.MUD.Driver.Core.Daemons.NetworkD;

/// <summary>
/// The Switchboard class is responsible for managing a collection of connected clients within
/// the networking subsystem. It maintains a mapping of unique client identifiers (GUIDs) to
/// their respective FluffyClient objects, facilitating efficient client management and interaction.
/// </summary>
/// <remarks>
/// The Switchboard serves as a central hub for tracking connected clients in a concurrent and
/// thread-safe manner, leveraging a ConcurrentDictionary to store client connections. This design
/// ensures scalability and consistency across network operations in a multithreaded environment.
/// </remarks>
public sealed class Switchboard
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
    /// <remarks>
    /// If the specified client is not found in the collection, the method logs a warning and exits.
    /// This method uses a thread-safe approach by locking during the removal process to ensure
    /// consistency in the ConnectedClients collection.
    /// </remarks>
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
    /// <remarks>
    /// This property provides a thread-safe count of the active connections stored in the Switchboard's
    /// collection. It reflects the current state of connected clients, enabling the networking subsystem
    /// to monitor and manage the system's load or allocate resources accordingly.
    /// </remarks>
    public int ClientCount => ConnectedClients.Count;

    /// <summary>
    /// Shuts down the Switchboard by disconnecting all connected clients and clearing internal resources.
    /// </summary>
    /// <remarks>
    /// This method iterates through all connected clients within the Switchboard and requests
    /// each client to disconnecting them with a specified reason. A debug log is recorded for each client
    /// disconnection, including its unique GUID and name. This operation is typically invoked
    /// during application termination or when the networking subsystem needs to be gracefully stopped.
    /// </remarks>
    internal void Shutdown()
    {
        foreach (var client in ConnectedClients.Values)
        {
            //client.FinalFlush();
            client.RequestDisconnect("Switchboard is shutting down.");
            Log.Debug($"Disconnecting client with GUID: {client.Guid}, {client.Name} from Switchboard.");
        }

        ConnectedClients.Clear();
    }

    /// <summary>
    /// Retrieves the FluffyClient instance associated with the given unique identifier (GUID).
    /// </summary>
    /// <param name="guid">The unique identifier (GUID) of the client to be retrieved from the collection.</param>
    /// <returns>The FluffyClient instance corresponding to the specified GUID, or null if no client with
    /// the provided GUID exists in the collection.</returns>
    /// <remarks>This method is used to look up and return a client based on its unique identifier.
    /// The operation is thread-safe, as the underlying ConnectedClients collection is a ConcurrentDictionary.
    /// </remarks>
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
    #endregion Client Management
}
/*
 *------------------------------------------------------------
 * (Switchboard.cs)
 * See License.txt for licensing information.
 *-----------------------------------------------------------
 */