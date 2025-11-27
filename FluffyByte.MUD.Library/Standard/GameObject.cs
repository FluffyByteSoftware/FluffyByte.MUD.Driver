/*
 * (GameObject.cs)
 *------------------------------------------------------------
 * Created - 11/26/2025 9:54:41 PM
 * Created by - Seliris
 *-------------------------------------------------------------
 */

using System.Collections.Concurrent;

namespace FluffyByte.MUD.Library.Standard;

/// <summary>
/// GameObject is the basic entity that all in-game objects derive from.
/// </summary>
public class GameObject : IAsyncDisposable, IGameObject
{
    /// <summary>
    /// Gets or sets the displayed name of the GameObject.
    /// </summary>
    public string Name { get; set; } = "Unnamed GameObject";

    /// <summary>
    /// Gets the collection of components attached to the game object. The collection is thread-safe and allows
    /// concurrent access and modification.
    /// </summary>
    /// <remarks>Use this property to enumerate, add, or remove components associated with the game object.
    /// Modifications to the collection are safe to perform from multiple threads. The order of components in the
    /// collection is not guaranteed.</remarks>
    public ConcurrentBag<IGameObjectComponent> Components { get; private set; } = [];

    /// <summary>
    /// Asynchronously releases resources used by the current instance.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> that represents the asynchronous dispose operation.</returns>
    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Renames the current object by setting its name to the specified value.
    /// </summary>
    /// <param name="newName">The new name to assign to the object. Cannot be null or empty.</param>
    public void Rename(string newName) => Name = newName;
}

/*
*------------------------------------------------------------
* (GameObject.cs)
* See License.txt for licensing information.
*-----------------------------------------------------------
*/