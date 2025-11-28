/*
 * (NetMonitor.cs)
 *------------------------------------------------------------
 * Created - 11/26/2025 7:59:58 PM
 * Created by - Seliris
 *-------------------------------------------------------------
 */

using FluffyByte.MUD.Driver.Core.Daemons.NetworkD.Clients;
using FluffyByte.MUD.Driver.FluffyTools;

namespace FluffyByte.MUD.Driver.Core.Daemons.NetworkD;

/// <summary>
/// Minion of the network daemon that monitors network connections.
/// </summary>
public sealed class Switchboard
{
    private readonly Dictionary<FluffyClient, string> _clientConnections = [];
    
    /// <summary>
    /// Starts the switchboard daemon.
    /// </summary>
    public async ValueTask RequestStart()
    {
        _clientConnections.Clear();
        
        try
        {
            if(_clientConnections.Count == 0) 
                Log.Debug($"Switchboard cleared previous client connections.");
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
        
        await Task.CompletedTask;
    }

    /// <summary>
    /// Gets the FluffyClient associated with the specified player name.
    /// </summary>
    /// <param name="name">The name of the account for the FluffyClient we want to find</param>
    /// <returns>FluffyClient</returns>
    public FluffyClient? GetClientFromPlayer(string name)
    {
        if (_clientConnections.Values.Any(nme => nme.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            return _clientConnections.FirstOrDefault(kvp => kvp.Value.Equals(name, 
                StringComparison.OrdinalIgnoreCase)).Key;
        }

        return null;
    }
    
    /// <summary>
    /// Adds a FluffyClient to the list of maintained clients.
    /// </summary>
    /// <param name="client">The FluffyClient to be added</param>
    public void AddClient(FluffyClient client)
    {
        _clientConnections.Add(client, client.Name);
    }

    /// <summary>
    /// Removes a FluffyClient from the list of maintained clients.
    /// </summary>
    /// <param name="client">The FluffyClient to remove.</param>
    public void RemoveClient(FluffyClient client)
    {
        _clientConnections.Remove(client);
    }
}

/*
*------------------------------------------------------------
* (NetMonitor.cs)
* See License.txt for licensing information.
*-----------------------------------------------------------
*/