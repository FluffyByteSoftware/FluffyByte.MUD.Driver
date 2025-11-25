/*
 * (Debugger.cs)
 *------------------------------------------------------------
 * Created - 11/19/2025 7:47:39 PM on $date$
 * Created by - Seliris
 *-------------------------------------------------------------
 */

using FluffyByte.MUD.Driver.Core.Types.Debug;

namespace FluffyByte.MUD.Driver.FluffyTools;

/// <summary>
/// Provides static methods for logging debug, informational, warning, and error messages with associated severity
/// levels.
/// </summary>
/// <remarks>The Debugger class offers a simple interface for writing diagnostic messages to the console,
/// categorized by severity. It supports logging plain messages as well as messages associated with exceptions. This
/// class is intended for use in application diagnostics and troubleshooting scenarios. All methods are thread-safe and
/// can be called from any context.</remarks>
public static class Log
{
    /// <summary>
    /// Writes a debug log entry with the specified severity and Message.
    /// </summary>
    /// <param name="severity">The severity level of the log entry. Determines the importance or type of the debug Message.</param>
    /// <param name="message">The Message to include in the log entry. Cannot be null.</param>
    public static void Scribe(DebugSeverity severity, string message)
        => CreateEnvelope(severity, message, null);

    /// <summary>
    /// Logs an informational Message with the Info severity level.
    /// </summary>
    /// <param name="message">The Message to log. Cannot be null.</param>
    public static void Info(string message)
        => CreateEnvelope(DebugSeverity.Info, message, null);

    /// <summary>
    /// Writes a debug-level Message to the log output.
    /// </summary>
    /// <remarks>Use this method to record diagnostic information that is useful during development or
    /// troubleshooting. Debug messages are typically not enabled in production environments.</remarks>
    /// <param name="message">The Message to log. Can be null or empty, but such messages may be ignored by some log listeners.</param>
    public static void Debug(string message)
        => CreateEnvelope(DebugSeverity.Debug, message, null);

    /// <summary>
    /// Logs the specified exception at the debug severity level.
    /// </summary>
    /// <param name="ex">The exception to log. Cannot be null.</param>
    public static void Debug(Exception ex)
        => CreateEnvelope(DebugSeverity.Debug, ex.Message, ex);

    /// <summary>
    /// Writes a debug-level log entry with the specified Message and associated exception.
    /// </summary>
    /// <param name="message">The Message to include in the debug log entry. This should describe the event or condition being logged.</param>
    /// <param name="ex">The exception to associate with the log entry. Can be used to provide stack trace and error details. Cannot be
    /// null.</param>
    public static void Debug(string message, Exception ex)
        => CreateEnvelope(DebugSeverity.Debug, message, ex);

    /// <summary>
    /// Logs a warning Message to the debug output with warning severity.
    /// </summary>
    /// <param name="message">The Message to log. Cannot be null.</param>
    public static void Warn(string message)
        => CreateEnvelope(DebugSeverity.Warn, message, null);

    /// <summary>
    /// Logs a warning Message for the specified exception.
    /// </summary>
    /// <param name="ex">The exception to log as a warning. Cannot be null.</param>
    public static void Warn(Exception ex)
        => CreateEnvelope(DebugSeverity.Warn, ex.Message, ex);

    /// <summary>
    /// Logs a warning Message with the specified exception details.
    /// </summary>
    /// <param name="message">The warning Message to log. This Message should describe the condition or event that triggered the warning.</param>
    /// <param name="ex">The exception associated with the warning. Provides additional context or stack trace information for the
    /// warning event. Cannot be null.</param>
    public static void Warn(string message, Exception ex)
        => CreateEnvelope(DebugSeverity.Warn, message, ex);

    /// <summary>
    /// Logs an error Message with error severity.
    /// </summary>
    /// <param name="message">The Message to log. Cannot be null.</param>
    public static void Error(string message)
        => CreateEnvelope(DebugSeverity.Error, message, null);

    /// <summary>
    /// Logs an error event using the specified exception information.
    /// </summary>
    /// <param name="ex">The exception that contains details about the error to log. Cannot be null.</param>
    public static void Error(Exception ex)
        => CreateEnvelope(DebugSeverity.Error, ex.Message, ex);

    /// <summary>
    /// Logs an error Message and associates it with the specified exception.
    /// </summary>
    /// <param name="message">The error Message to log. This Message should describe the error condition.</param>
    /// <param name="ex">The exception related to the error. Cannot be null.</param>
    public static void Error(string message, Exception ex)
        => CreateEnvelope(DebugSeverity.Error, message, ex);

    /// <summary>
    /// Creates a log envelope with the specified severity, Message, and optional exception information, and writes it
    /// to the console.
    /// </summary>
    /// <remarks>The console output color is set based on the specified severity to visually distinguish log
    /// levels. This method does not persist the log entry; it only writes to the console.</remarks>
    /// <param name="severity">The severity level of the log entry. Determines the importance and formatting of the log Message.</param>
    /// <param name="message">The Message to include in the log entry. Provides details about the event or error being logged.</param>
    /// <param name="ex">An optional exception to include in the log entry. If not null, exception details are added to the envelope.</param>
    private static void CreateEnvelope(DebugSeverity severity, string message, Exception? ex = null)
    {
        LogEnvelope envelope;
        ExceptionInfo ei;

        if (ex is not null)
        {
            ei = new(ex);
            envelope = new(severity, message, ei);
        }
        else
        {
            envelope = new(severity, message, null);
        }

        Console.ForegroundColor = severity switch
        {
            DebugSeverity.Debug => ConsoleColor.Green,
            DebugSeverity.Info => ConsoleColor.White,
            DebugSeverity.Warn => ConsoleColor.Yellow,
            DebugSeverity.Error => ConsoleColor.Red,
            _ => ConsoleColor.Blue,
        };

        Console.WriteLine(envelope.ToString());
        //FileManager.AddEnvelopeToQueue(envelope);
        Console.ResetColor();
    }
    
}

/*
*------------------------------------------------------------
* (Debugger.cs)
* See License.txt for licensing information.
*-----------------------------------------------------------
*/