/*
 * (ExceptionInfo.cs)
 *------------------------------------------------------------
 * Created - 11/19/2025 10:27:55 PM on $date$
 * Created by - Seliris
 *-------------------------------------------------------------
 */

namespace FluffyByte.MUD.Driver.Core.Types.Debug;

/// <summary>
/// Represents serializable information about an exception, including its type, Message, stack trace, and any inner
/// exceptions.
/// </summary>
/// <remarks>This class is useful for capturing and transferring exception details in a format that can be easily
/// logged, serialized, or transmitted across application boundaries. The inner exception chain is limited to a maximum
/// depth of 10 to prevent excessively deep or recursive structures.</remarks>
public sealed class ExceptionInfo
{
    /// <summary>
    /// Gets the type identifier for the current instance.
    /// </summary>
    public string Type { get; }
    /// <summary>
    /// Gets the Message content associated with this instance.
    /// </summary>
    public string Message { get; }
    /// <summary>
    /// Gets a string representation of the immediate frames on the call stack at the time the exception was thrown.
    /// </summary>
    /// <remarks>The stack trace provides information that can be useful for debugging, such as the sequence
    /// of method calls that led to the exception. The value may be null if no stack trace is available.</remarks>
    public string? StackTrace { get; }
    /// <summary>
    /// Gets the exception information for the inner exception, if one exists.
    /// </summary>
    public ExceptionInfo? Inner { get; }

    /// <summary>
    /// Initializes a new instance of the ExceptionInfo class using the specified exception.
    /// </summary>
    /// <remarks>This constructor captures the type, Message, and stack trace of the provided exception. If
    /// the exception has inner exceptions, up to 10 levels of inner exception information are recursively included.
    /// This helps in representing complex exception chains for diagnostic or logging purposes.</remarks>
    /// <param name="ex">The exception to extract information from. Cannot be null.</param>
    public ExceptionInfo(Exception ex)
    {
        Type         =  ex.GetType().FullName ?? "UnknownExceptionType";
        Message      =  ex.Message;
        StackTrace   =  ex.StackTrace;

        int depth = 0;

        while(ex.InnerException is not null && depth < 10)
        {
            Inner = new ExceptionInfo(ex.InnerException);
            ex = ex.InnerException;

            depth++;
        }
    }
}

/*
*------------------------------------------------------------
* (ExceptionInfo.cs)
* See License.txt for licensing information.
*-----------------------------------------------------------
*/