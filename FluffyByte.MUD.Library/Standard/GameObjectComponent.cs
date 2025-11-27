/*
 * (GameObjectComponent.cs)
 *------------------------------------------------------------
 * Created - 11/26/2025 10:23:45 PM
 * Created by - Seliris
 *-------------------------------------------------------------
 */

namespace FluffyByte.MUD.Library.Standard;

/// <summary>
/// Represents a component of a GameObject, providing additional functionality or data.
/// </summary>
public class GameObjectComponent(GameObject owner)
{

    /// <summary>
    /// Represents the parent GameObject that owns this component.
    /// </summary>
    public GameObject Owner { get; private set; } = owner;

}

/*
*------------------------------------------------------------
* (GameObjectComponent.cs)
* See License.txt for licensing information.
*-----------------------------------------------------------
*/