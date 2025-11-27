/*
 * (IGameObject.cs)
 *------------------------------------------------------------
 * Created - 11/26/2025 10:26:46 PM
 * Created by - Seliris
 *-------------------------------------------------------------
 */

using System.Collections.Concurrent;

namespace FluffyByte.MUD.Library.Standard;

/// <summary>
/// Interface for all game objects
/// </summary>
public interface IGameObject
{
    /// <summary>
    /// Gets the name associated with the current game object.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the collection of components attached to the game object.
    /// </summary>
    /// <remarks>The returned collection is thread-safe and may be modified concurrently by multiple threads.
    /// Components in the collection implement the IGameObjectComponent interface and represent additional behaviors or
    /// data associated with the game object.</remarks>
    ConcurrentBag<IGameObjectComponent> Components { get; }

}

/*
*------------------------------------------------------------
* (IGameObject.cs)
* See License.txt for licensing information.
*-----------------------------------------------------------
*/