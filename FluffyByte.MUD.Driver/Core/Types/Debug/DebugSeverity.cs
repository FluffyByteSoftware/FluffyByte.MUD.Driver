/*
 * (DebugSeverity.cs)
 *------------------------------------------------------------
 * Created - 11/19/2025 10:25:56 PM on $date$
 * Created by - Seliris
 *-------------------------------------------------------------
 */

namespace FluffyByte.MUD.Driver.Core.Types.Debug;

/// <summary>
/// Specifies the severity level of a debug or log Message.
/// </summary>
/// <remarks>Use this enumeration to categorize log messages by their importance or urgency. The values range from
/// detailed debugging information to critical errors that require immediate attention.</remarks>
public enum DebugSeverity
{
    /// <summary>
    /// Debug mode message, typically used for development and troubleshooting.
    /// </summary>
    Debug,
    /// <summary>
    /// Info mode message, used for general informational purposes.
    /// </summary>
    Info,
    /// <summary>
    /// Warn mode message, used for warning conditions that are not errors but may require attention.
    /// </summary>
    Warn,
    /// <summary>
    /// Error mode message, used for error conditions that indicate a failure in a specific operation.
    /// </summary>
    Error,
}

/*
*------------------------------------------------------------
* (DebugSeverity.cs)
* See License.txt for licensing information.
*-----------------------------------------------------------
*/