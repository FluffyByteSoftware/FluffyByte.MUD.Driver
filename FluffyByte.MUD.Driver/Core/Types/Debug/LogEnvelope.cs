/*
 * (LogEnvelope.cs)
 *------------------------------------------------------------
 * Created - 11/19/2025 10:25:19 PM on $date$
 * Created by - Seliris
 *-------------------------------------------------------------
 */

using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FluffyByte.MUD.Driver.Core.Types.Debug;

/// <summary>
/// Represents a structured log entry containing Message details, severity, timestamp, and contextual information about
/// the log event.
/// </summary>
/// <remarks>Use this type to encapsulate all relevant information for a single log event, including the Message,
/// severity level, optional exception details, and the source location in code. This structure is intended for use in
/// logging frameworks or diagnostic tools that require rich log context.</remarks>
public readonly record struct LogEnvelope
{
    /// <summary>
    /// Gets the date and time when the event occurred or the data was recorded.
    /// </summary>
    public DateTime Timestamp { get; init; }
    /// <summary>
    /// Gets the severity level associated with the debug Message.
    /// </summary>
    public DebugSeverity Severity { get; init; }
    /// <summary>
    /// Gets the Message content associated with this instance.
    /// </summary>
    public string Message { get; init; }
    /// <summary>
    /// Gets information about the exception that occurred during the operation, if any.
    /// </summary>
    public ExceptionInfo? Exception { get; init; }
    /// <summary>
    /// Gets the path of the source file associated with this instance.
    /// </summary>
    public string SourceFile { get; init; }
    /// <summary>
    /// Gets the line number in the source file associated with this instance.
    /// </summary>
    public int SourceLine { get; init; }
    /// <summary>
    /// Gets the identifier of the user or process that initiated the operation.
    /// </summary>
    public string Caller { get; init; }

    /// <summary>
    /// Initializes a new instance of the LogEnvelope class with the specified severity, Message, and optional exception
    /// and caller information.
    /// </summary>
    /// <param name="severity">The severity level of the log entry, indicating its importance or type.</param>
    /// <param name="message">The log Message describing the event or condition being logged.</param>
    /// <param name="exception">Optional. Additional exception information to include with the log entry, or null if not applicable.</param>
    /// <param name="sourceFile">Optional. The path of the source file where the log entry was generated. Defaults to an empty string.</param>
    /// <param name="sourceLine">Optional. The line number in the source file where the log entry was generated. Defaults to 0.</param>
    /// <param name="caller">Optional. The name of the method or member that generated the log entry. Defaults to an empty string.</param>
    public LogEnvelope(
        DebugSeverity severity,
        string message,
        ExceptionInfo? exception = null,
        string sourceFile = "",
        int sourceLine = 0,
        string caller = "")
    {
        Timestamp = DateTime.UtcNow;
        Severity = severity;
        Message = message;
        Exception = exception;
        SourceFile = sourceFile;
        SourceLine = sourceLine;
        Caller = caller;
    }

    /// <summary>
    /// Returns a formatted string that represents the current log entry, including details such as timestamp, severity,
    /// Message, and exception information if present.
    /// </summary>
    /// <remarks>The returned string is formatted for readability and may include additional context such as
    /// source file and caller information if available. This method is intended for diagnostic or display purposes and
    /// is not suitable for machine parsing.</remarks>
    /// <returns>A multi-line string containing the log entry details. If an exception is associated with the entry, the string
    /// includes exception type, Message, and stack trace; otherwise, it summarizes the log Message and metadata.</returns>
    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.AppendLine($"╔════════════════════════════════════════════════════════════");
        sb.AppendLine($"║ Timestamp : {Timestamp:yyyy-MM-dd HH:mm:ss.fff} UTC");
        sb.AppendLine($"║ Severity  : {Severity}");
        sb.AppendLine($"║ Message   : {Message}");

        if (!string.IsNullOrEmpty(SourceFile))
            sb.AppendLine($"║ Source    : {SourceFile}:{SourceLine}");

        if (!string.IsNullOrEmpty(Caller))
            sb.AppendLine($"║ Caller    : {Caller}");

        if (Exception is null)
        {
            sb.AppendLine("╚════════════════════════════════════════════════════════════");
            return sb.ToString();
        }

        sb.AppendLine("║");
        sb.AppendLine("║ Exception Details:");

        // Walk inner exceptions (max depth = 10)
        var current = Exception;
        int depth = 0;

        while (current is not null && depth < 10)
        {
            string indent = new(' ', depth * 2);

            sb.AppendLine($"║ {indent}Type    : {current.Type}");
            sb.AppendLine($"║ {indent}Message : {current.Message}");
            sb.AppendLine($"║ {indent}Stack   :");

            if (current.StackTrace is null)
            {
                sb.AppendLine($"║ {indent}  <No stack trace available>");
            }
            else
            {
                foreach (var line in current.StackTrace.Split('\n'))
                    sb.AppendLine($"║ {indent}  {line}");
            }

            current = current.Inner;   // Assuming ExceptionInfo.Inner exists.
            depth++;

            if (current is not null)
            {
                sb.AppendLine($"║");
                sb.AppendLine($"║ {indent}--- Inner Exception ---");
                sb.AppendLine($"║");
            }
        }

        sb.AppendLine("╚════════════════════════════════════════════════════════════");

        return sb.ToString();
    }

}

/*
*------------------------------------------------------------
* (LogEnvelope.cs)
* See License.txt for licensing information.
*-----------------------------------------------------------
*/