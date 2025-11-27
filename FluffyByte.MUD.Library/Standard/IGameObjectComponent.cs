/*
 * (IGameObjectComponent.cs)
 *------------------------------------------------------------
 * Created - 11/26/2025 10:25:49 PM
 * Created by - Seliris
 *-------------------------------------------------------------
 */

namespace FluffyByte.MUD.Library.Standard;

/// <summary>
/// Defines the contract for a component that is attached to a game object within the game engine.
/// </summary>
/// <remarks>Implementations of this interface represent modular behaviors or data that can be associated with a
/// game object. Components typically interact with their owning game object to extend functionality or respond to game
/// events.</remarks>
public interface IGameObjectComponent
{
    /// <summary>
    /// Gets the GameObject that owns this GameObjectComponent.
    /// </summary>
    GameObject Owner { get; }
}

/*
*------------------------------------------------------------
* (IGameObjectComponent.cs)
* See License.txt for licensing information.
*-----------------------------------------------------------
*/