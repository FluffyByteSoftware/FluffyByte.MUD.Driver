/*
 * (AccountManager.cs)
 *------------------------------------------------------------
 * Created - 9:54:03 PM
 * Created by - Jacob Chacko
 *------------------------------------------------------------
 */

namespace FluffyByte.MUD.Driver.Core.Daemons.Networking;

public class AccountManager
{
    private static readonly Lazy<AccountManager> Singleton = new(() => new AccountManager());

    /// <summary>Gets the singleton instance of the <see cref="AccountManager"/> class.
    /// This property ensures that only one instance of <see cref="AccountManager"/> exists throughout the
    /// application, providing a global point of access.
    /// </summary>
    public static AccountManager Instance => Singleton.Value;
    
    
}
/*
 *------------------------------------------------------------
 * (AccountManager.cs)
 * See License.txt for licensing information.
 *-----------------------------------------------------------
 */