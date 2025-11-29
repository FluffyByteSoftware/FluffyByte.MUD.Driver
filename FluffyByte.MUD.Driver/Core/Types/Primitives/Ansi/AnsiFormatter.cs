/*
 * (AnsiString.cs)
 *------------------------------------------------------------
 * Created - 8:36:43 AM
 * Created by - Jacob Chacko
 *------------------------------------------------------------
 */
namespace FluffyByte.MUD.Driver.Core.Types.Primitives.Ansi;

/// <summary>
/// Wraps a string (Message) up and can be used to parse or return the underlying message with escape sequences
/// replacing our local coloring system.
/// </summary>
public struct AnsiFormatter
{
    /// <summary>
    /// Returns a string with ANSI escape sequences replaced by their corresponding terminal formatting codes.
    /// </summary>
    /// <param name="content">The string to parse for ANSI escape sequences.</param>
    /// <returns>The parsed string with ANSI escape sequences replaced by terminal formatting codes.</returns>
    public static string Parse(string content)
    {
        if (string.IsNullOrEmpty(content))
            return content;

        var ansiMap = new Dictionary<string, string>
        {
            // Foreground colors
            { "%^BLACK%^", "\u001b[30m" },
            { "%^RED%^", "\u001b[31m" },
            { "%^GREEN%^", "\u001b[32m" },
            { "%^YELLOW%^", "\u001b[33m" },
            { "%^BLUE%^", "\u001b[34m" },
            { "%^MAGENTA%^", "\u001b[35m" },
            { "%^CYAN%^", "\u001b[36m" },
            { "%^WHITE%^", "\u001b[37m" },

            // Background colors
            { "%^BG_BLACK%^", "\u001b[40m" },
            { "%^BG_RED%^", "\u001b[41m" },
            { "%^BG_GREEN%^", "\u001b[42m" },
            { "%^BG_YELLOW%^", "\u001b[43m" },
            { "%^BG_BLUE%^", "\u001b[44m" },
            { "%^BG_MAGENTA%^", "\u001b[45m" },
            { "%^BG_CYAN%^", "\u001b[46m" },
            { "%^BG_WHITE%^", "\u001b[47m" },

            // Formatting
            { "%^BOLD%^", "\u001b[1m" },
            { "%^ITALIC%^", "\u001b[3m" },
            { "%^RESET%^", "\u001b[0m" }
        };

        var result = ansiMap.Aggregate(content, 
            (current, kvp) => current.Replace(kvp.Key, kvp.Value));

        return result;
    }
}
/*
 *------------------------------------------------------------
 * (AnsiString.cs)
 * See License.txt for licensing information.
 *-----------------------------------------------------------
 */