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
    #region Variables
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
    
    // Fixed-width box drawing infrastructure

    private const char TL = '╔';
    private const char TR = '╗';

    private const char BL = '╚';
    private const char BR = '╝';

    private const char MR = '╢';
    private const char ML = '╟';

    private const char HL = '═';
    private const char VL = '║';

    private const int BOX_WIDTH = 80;
    private const int INNER_WIDTH = BOX_WIDTH - 4; // | + space + content + space + |
    #endregion

    #region Constructor
    public LogEnvelope(
        DebugSeverity severity,
        string message,
        ExceptionInfo? exception = null,
        string sourceFile = "",
        int sourceLine = 0,
        string caller = ""
        )
    {
        Timestamp = DateTime.UtcNow;
        Severity = severity;
        Message = message;
        Exception = exception;
        SourceFile = sourceFile;
        SourceLine = sourceLine;
        Caller = caller;
    }
    #endregion


    private static string BoxLine(char left, char right)
        => $"{left}{new string('═', BOX_WIDTH - 2)}{right}";

    private static string BoxContent(string content)
    {
        if (content.Length > INNER_WIDTH)
            content = content[..INNER_WIDTH];

        return $"{VL} {content,-INNER_WIDTH} {VL}";
    }

    private static IEnumerable<string> WrapField(string label, string value)
    {
        string prefix = $"{label,-9}: ";
        int available = INNER_WIDTH - prefix.Length;

        if (available < 10)
            available = 10;

        for (int i = 0; i < value.Length; i += available)
        {
            string chunk = value.Substring(i, Math.Min(available, value.Length - i));

            if (i == 0)
                yield return BoxContent(prefix + chunk);
            else
                yield return BoxContent(new string(' ', prefix.Length) + chunk);
        }
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.AppendLine(BoxLine(TL, TR));
        sb.AppendLine(BoxContent($"Timestamp : {Timestamp:yyyy-MM-dd HH:mm:ss.fff} UTC"));
        sb.AppendLine(BoxContent($"Severity  : {Severity}"));
        sb.AppendLine(BoxContent($"Message   : {Message}"));
        sb.AppendLine(BoxLine(ML, MR));

        if (!string.IsNullOrEmpty(SourceFile))
        {
            string fullPath = $"{SourceFile}:{SourceLine}";

            foreach (string line in WrapField("Source", fullPath))
                sb.AppendLine(line);

            if (!string.IsNullOrEmpty(Caller))
                sb.AppendLine(BoxContent($"Caller    : {Caller}"));
        }

        if (Exception is null)
        {
            sb.AppendLine(BoxLine(BL, BR));
            return sb.ToString();
        }

        sb.AppendLine(BoxContent(""));
        sb.AppendLine(BoxContent("Exception Details:"));

        var current = Exception;
        int depth = 0;

        while (current is not null && depth < 10)
        {
            string indent = new(' ', depth * 2);

            sb.AppendLine(BoxContent($"{indent}Type    : {current.Type}"));
            sb.AppendLine(BoxContent($"{indent}Message : {current.Message}"));
            sb.AppendLine(BoxContent($"{indent}Stack   :"));

            if (current.StackTrace is null)
            {
                sb.AppendLine(BoxContent($"{indent}  <No stack trace available>"));
            }
            else
            {
                foreach (var line in current.StackTrace.Split('\n'))
                    sb.AppendLine(BoxContent($"{indent}  {line}"));
            }

            current = current.Inner;
            depth++;

            if (current is not null)
            {
                sb.AppendLine(BoxContent(""));
                sb.AppendLine(BoxContent($"{indent}--- Inner Exception ---"));
                sb.AppendLine(BoxContent(""));
            }
        }

        sb.AppendLine(BoxLine(BL, BR));
        return sb.ToString();
    }
}


/*
*------------------------------------------------------------
* (LogEnvelope.cs)
* See License.txt for licensing information.
*-----------------------------------------------------------
*/