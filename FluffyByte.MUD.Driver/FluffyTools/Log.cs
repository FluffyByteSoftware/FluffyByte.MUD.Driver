/*
 * (Debugger.cs)
 *------------------------------------------------------------
 * Created - 11/19/2025 7:47:39 PM on $date$
 * Created by - Seliris
 *-------------------------------------------------------------
 */

using System.Runtime.CompilerServices;
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
    /// Marks whether or not the Logger should display Debug messages or not.
    /// Useful for debugging.
    /// </summary>
    public static bool DebugModeEnabled { get; set; } = true;
    #region Public Methods
    public static void DisplayFileContents(string fileContents)
    {
        Console.WriteLine("File Contents: ");
        Console.WriteLine($"{fileContents}");
    }

    public static void Scribe(
        DebugSeverity severity,
        string message,
        Exception? ex = null,
        [CallerLineNumber] int line = 0,
        [CallerMemberName] string member = "",
        [CallerFilePath] string file = "")
        => CreateEnvelope(severity, message, ex, line, member, file);

    public static void Info(
        string message,
        [CallerLineNumber] int line = 0,
        [CallerMemberName] string member = "",
        [CallerFilePath] string file = "")
        => CreateEnvelope(DebugSeverity.Info, message, null, line, member, file);

    public static void Debug(
        string message,
        [CallerLineNumber] int line = 0,
        [CallerMemberName] string member = "",
        [CallerFilePath] string file = "")
    {
        if(!DebugModeEnabled)
            return;

        CreateEnvelope(DebugSeverity.Debug, message, null, line, member, file);
    }

    public static void Debug(
        Exception ex,
        [CallerLineNumber] int line = 0,
        [CallerMemberName] string member = "",
        [CallerFilePath] string file = "")
    {
        if (!DebugModeEnabled)
            return;

        CreateEnvelope(DebugSeverity.Debug, ex.Message, ex, line, member, file);
    }

    public static void Debug(
        string message,
        Exception ex,
        [CallerLineNumber] int line = 0,
        [CallerMemberName] string member = "",
        [CallerFilePath] string file = "")
    {
        if(!DebugModeEnabled)
            return;
        CreateEnvelope(DebugSeverity.Debug, message, ex, line, member, file);
    }

    public static void Warn(
        string message,
        [CallerLineNumber] int line = 0,
        [CallerMemberName] string member = "",
        [CallerFilePath] string file = "")
        => CreateEnvelope(DebugSeverity.Warn, message, null, line, member, file);

    public static void Warn(
        Exception ex,
        [CallerLineNumber] int line = 0,
        [CallerMemberName] string member = "",
        [CallerFilePath] string file = "")
        => CreateEnvelope(DebugSeverity.Warn, ex.Message, ex, line, member, file);

    public static void Warn(
        string message,
        Exception ex,
        [CallerLineNumber] int line = 0,
        [CallerMemberName] string member = "",
        [CallerFilePath] string file = "")
        => CreateEnvelope(DebugSeverity.Warn, message, ex, line, member, file);

    public static void Error(
        string message,
        [CallerLineNumber] int line = 0,
        [CallerMemberName] string member = "",
        [CallerFilePath] string file = "")
        => CreateEnvelope(DebugSeverity.Error, message, null, line, member, file);

    public static void Error(
        Exception ex,
        [CallerLineNumber] int line = 0,
        [CallerMemberName] string member = "",
        [CallerFilePath] string file = "")
        => CreateEnvelope(DebugSeverity.Error, ex.Message, ex, line, member, file);

    public static void Error(
        string message,
        Exception ex,
        [CallerLineNumber] int line = 0,
        [CallerMemberName] string member = "",
        [CallerFilePath] string file = "")
        => CreateEnvelope(DebugSeverity.Error, message, ex, line, member, file);
    #endregion

    #region Private Methods
    private static void CreateEnvelope(
        DebugSeverity severity,
        string message,
        Exception? ex,
        int line,
        string member,
        string file)
    {
        var envelope = new LogEnvelope(
            severity,
            message,
            ex != null ? new ExceptionInfo(ex) : null,
            file,
            line,
            member
        );

        ConsoleColor fg = Console.ForegroundColor;

        Console.ForegroundColor = severity switch
        {
            DebugSeverity.Debug => ConsoleColor.Green,
            DebugSeverity.Info => ConsoleColor.White,
            DebugSeverity.Warn => ConsoleColor.Yellow,
            DebugSeverity.Error => ConsoleColor.Red,
            _ => ConsoleColor.Blue,
        };

        Console.WriteLine(envelope.ToString());

        Console.ForegroundColor = fg;
    }
    #endregion
}


/*
*------------------------------------------------------------
* (Debugger.cs)
* See License.txt for licensing information.
*-----------------------------------------------------------
*/