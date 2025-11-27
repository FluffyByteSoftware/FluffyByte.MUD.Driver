/*
 * (ClientState.cs)
 *------------------------------------------------------------
 * Created - 11/27/2025 8:55:36 AM
 * Created by - Seliris
 *-------------------------------------------------------------
 */

namespace FluffyByte.MUD.Driver.Core.Types.Daemons.Networking;

/// <summary>
/// Represents the various states a FluffyClient be in during its lifecycle.
/// </summary>
public enum ClientState
{

    /// <summary>
    /// THe current client is disconnected.
    /// </summary>
    Disconnected,
    /// <summary>
    /// The current client is connecting.
    /// </summary>
    Connecting,
    /// <summary>
    /// The current client has connected but has not yet been served authentication.
    /// </summary>
    Connected,
    /// <summary>
    /// The current client has been served authentication and is in the process of authenticating.
    /// </summary>
    Authenticating,
    /// <summary>
    /// The current client has been authenticated successfully, and is now selecting their player.
    /// </summary>
    Authenticated,
    /// <summary>
    /// The current client is loaded into the game simulation.
    /// </summary>
    Playing,
    /// <summary>
    /// The client is in an error state.
    /// </summary>
    Error
}

/*
*------------------------------------------------------------
* (ClientState.cs)
* See License.txt for licensing information.
*-----------------------------------------------------------
*/