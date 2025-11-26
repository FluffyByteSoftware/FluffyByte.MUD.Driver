/*
 * (LogEnvelope.cs)
 *------------------------------------------------------------
 * Created - 11/19/2025 10:25:19 PM on $date$
 * Created by - Seliris
 *-------------------------------------------------------------
 */

using System.Text;

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
    /// <summary>
    /// Initializes a new instance of the LogEnvelope class with the specified severity, message, and optional exception
    /// and source information.
    /// </summary>
    /// <param name="severity">The severity level of the log entry, indicating its importance or type.</param>
    /// <param name="message">The message to be logged. This should describe the event or condition being recorded.</param>
    /// <param name="exception">Optional exception information associated with the log entry. Specify null if no exception is related.</param>
    /// <param name="sourceFile">The source file path where the log entry originated. If not specified, defaults to an empty string.</param>
    /// <param name="sourceLine">The line number in the source file where the log entry was generated. If not specified, defaults to 0.</param>
    /// <param name="caller">The name of the method or member that generated the log entry. If not specified, defaults to an empty string.</param>
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

    #region Box Drawing Helpers

    private static string BoxLine(char left, char right)
        => $"{left}{new string(HL, BOX_WIDTH - 2)}{right}";

    private static string BoxContent(string content)
        => $"{VL} {content,-INNER_WIDTH} {VL}";

    /// <summary>
    /// Wraps a labeled field value across multiple lines, maintaining proper indentation.
    /// </summary>
    /// <param name="label">The field label (will be padded to labelWidth).</param>
    /// <param name="value">The value to wrap.</param>
    /// <param name="labelWidth">Width to pad the label to (default 9 for standard fields).</param>
    /// <param name="baseIndent">Additional indentation prefix for nested content.</param>
    private static IEnumerable<string> WrapField(
        string label,
        string value,
        int labelWidth = 9,
        string baseIndent = "")
    {
        string prefix = $"{baseIndent}{label.PadRight(labelWidth)}: ";
        string continuation = new(' ', prefix.Length);
        int available = INNER_WIDTH - prefix.Length;

        if (available < 10)
            available = 10;

        // Handle empty or whitespace values
        if (string.IsNullOrEmpty(value))
        {
            yield return BoxContent(prefix);
            yield break;
        }

        // Split by existing newlines first, then wrap each segment
        var segments = value.Split('\n');

        bool firstSegment = true;
        foreach (var segment in segments)
        {
            string trimmed = segment.TrimEnd('\r');

            if (trimmed.Length == 0)
            {
                yield return BoxContent(firstSegment ? prefix : continuation);
                firstSegment = false;
                continue;
            }

            for (int i = 0; i < trimmed.Length; i += available)
            {
                string chunk = trimmed.Substring(i, Math.Min(available, trimmed.Length - i));

                if (firstSegment && i == 0)
                    yield return BoxContent(prefix + chunk);
                else
                    yield return BoxContent(continuation + chunk);
            }

            firstSegment = false;
        }
    }

    /// <summary>
    /// Wraps raw text (like stack traces) with a fixed indentation prefix.
    /// </summary>
    private static IEnumerable<string> WrapText(string text, string indent = "")
    {
        int available = INNER_WIDTH - indent.Length;

        if (available < 10)
            available = 10;

        var lines = text.Split('\n');

        foreach (var line in lines)
        {
            string trimmed = line.TrimEnd('\r');

            if (trimmed.Length == 0)
            {
                yield return BoxContent(indent);
                continue;
            }

            for (int i = 0; i < trimmed.Length; i += available)
            {
                string chunk = trimmed.Substring(i, Math.Min(available, trimmed.Length - i));
                yield return BoxContent(indent + chunk);
            }
        }
    }

    private static void AppendField(StringBuilder sb, string label, string value, int labelWidth = 9, string indent = "")
    {
        foreach (var line in WrapField(label, value, labelWidth, indent))
            sb.AppendLine(line);
    }

    #endregion

    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.AppendLine(BoxLine(TL, TR));
        AppendField(sb, "Timestamp", $"{Timestamp:yyyy-MM-dd HH:mm:ss.fff} UTC");
        AppendField(sb, "Severity", Severity.ToString());
        AppendField(sb, "Message", Message);
        sb.AppendLine(BoxLine(ML, MR));

        if (!string.IsNullOrEmpty(SourceFile))
        {
            AppendField(sb, "Source", $"{SourceFile}:{SourceLine}");

            if (!string.IsNullOrEmpty(Caller))
                AppendField(sb, "Caller", Caller);
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

            sb.AppendLine(BoxContent(""));
            AppendField(sb, "Type", current.Type, 7, indent);
            AppendField(sb, "Message", current.Message, 7, indent);
            sb.AppendLine(BoxContent($"{indent}Stack:"));

            if (string.IsNullOrEmpty(current.StackTrace))
            {
                sb.AppendLine(BoxContent($"{indent}  <No stack trace available>"));
            }
            else
            {
                foreach (var line in WrapText(current.StackTrace, indent + "  "))
                    sb.AppendLine(line);
            }

            current = current.Inner;
            depth++;

            if (current is not null)
            {
                sb.AppendLine(BoxContent(""));
                sb.AppendLine(BoxContent($"{indent}--- Inner Exception ---"));
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