/*
 * (Grammar.cs)
 *------------------------------------------------------------
 * Created - 11/25/2025 11:50:40 PM
 * Created by - Seliris
 *-------------------------------------------------------------
 */

using System.Text;

namespace FluffyByte.MUD.Driver.FluffyTools;

public static partial class Grammar
{
    #region Articles
    private static readonly HashSet<char> _vowels =
        [
            'a',
            'e',
            'i',
            'o',
            'u',
            'A',
            'E',
            'I',
            'O',
            'U'
        ];

    // Words that look they start with vowels but sound like consonants
    private static readonly HashSet<string> _anExceptions =
        [
            "one", "once", "united", "unique", "unicorn", "union", "unit", "universe",
            "universal", "university", "uniform", "unison", "uranium", "urine", "usable",
            "usage", "use", "used", "useful", "user", "usual", "usually", "usurp",
            "utensil", "utility", "utopia", "european", "eulogy", "euphemism", "euphoria",
            "euthanasia", "ewe", "ewes"
        ];

    private static readonly HashSet<string> _aExceptions =
        [
            "heir", "honest", "honor", "honour", "hour", "hourly", "herb"
        ];

    public static string Article(string word)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(word))
                return "a";

            string lower = word.ToLowerInvariant();
            string? firstWord = lower.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();

            if (firstWord == null)
            {
                throw new NullReferenceException(nameof(firstWord));
            }

            if (_anExceptions.Any(e => firstWord.StartsWith(e)))
                return "a";

            if (_aExceptions.Any(e => firstWord.StartsWith(e)))
                return "an";

            return _vowels.Contains(word[0]) ? "an" : "a";
        }
        catch (Exception ex)
        {
            Log.Error(ex);
            throw;
        }
    }

    /// <summary>
    /// Prepends the appropriate article to a word.
    /// </summary>
    /// <param name="word">The word to be prepended with an article.</param>
    /// <returns>"sword" -> "a sword", "apple" -> "an apple"</returns>
    public static string WithArticle(string word)
        => $"{Article(word)} {word}";

    public static string WithArticleCapitalized(string word)
        => $"{WithArticleCapitalized(Article(word))} {word}";
    #endregion

    #region Pluralization
    private static readonly Dictionary<string, string> _irregularPlurals = new(StringComparer.OrdinalIgnoreCase)
    {
        // People
        ["man"] = "men",
        ["woman"] = "women",
        ["child"] = "children",
        ["person"] = "people",
        ["foot"] = "feet",
        ["tooth"] = "teeth",
        ["wife"] = "wives",

        // Animals
        ["fish"] = "fish",
        ["sheep"] = "sheep",
        ["deer"] = "deer",
        ["moose"] = "moose",
        ["swine"] = "swine",
        ["buffalo"] = "buffalo",
        ["shrimp"] = "shrimp",
        ["trout"] = "trout",
        ["salmon"] = "salmon",
        ["wolf"] = "wolves",
        ["calf"] = "calves",
        ["goose"] = "geese",
        ["mouse"] = "mice",
        ["louse"] = "lice",
        ["ox"] = "oxen",

        // Fantasy creatures
        ["dwarf"] = "dwarves",
        ["elf"] = "elves",
        ["half"] = "halves",
        ["knife"] = "knives",
        ["leaf"] = "leaves",
        ["life"] = "lives",
        ["loaf"] = "loaves",
        ["self"] = "selves",
        ["shelf"] = "shelves",
        ["thief"] = "thieves",
        ["scarf"] = "scarves",

        // Latin/Greek
        ["cactus"] = "cacti",
        ["focus"] = "foci",
        ["fungus"] = "fungi",
        ["nucleus"] = "nuclei",
        ["radius"] = "radii",
        ["stimulus"] = "stimuli",
        ["alumnus"] = "alumni",
        ["criterion"] = "criteria",
        ["phenomenon"] = "phenomena",
        ["datum"] = "data",
        ["medium"] = "media",
        ["bacterium"] = "bacteria",
        ["curriculum"] = "curricula",
        ["memorandum"] = "memoranda",
        ["stadium"] = "stadia",

        // Other irregulars
        ["die"] = "dice",
        ["penny"] = "pence",
        ["index"] = "indices",
        ["appendix"] = "appendices",
        ["matrix"] = "matrices",
        ["vertex"] = "vertices",
        ["axis"] = "axes",
        ["crisis"] = "crises",
        ["thesis"] = "theses",
        ["analysis"] = "analyses",
        ["diagnosis"] = "diagnoses",
        ["oasis"] = "oases",
        ["parenthesis"] = "parentheses",
    };

    private static readonly Dictionary<string, string> _irregularSingulars;

    static Grammar()
    {
        _irregularSingulars = new(StringComparer.OrdinalIgnoreCase);

        foreach (var kvp in _irregularPlurals)
        {
            _irregularSingulars.TryAdd(kvp.Value, kvp.Key);
        }
    }

    public static string Pluralize(string noun)
    {
        if (string.IsNullOrWhiteSpace(noun))
            return noun;

        // Check irregular pronouns
        if (_irregularPlurals.TryGetValue(noun, out string? irregular))
            return MatchCase(noun, irregular);

        string lower = noun.ToLowerInvariant();

        if (lower.EndsWith('s') || lower.EndsWith('x') || lower.EndsWith('z')
            || lower.EndsWith("ch") || lower.EndsWith("sh"))
            return noun + "es";

        if (lower.EndsWith('y') && noun.Length > 1 && !_vowels.Contains(noun[^2]))
            return noun[..^1] + "ies";

        if (lower.EndsWith('f'))
            return noun[..^1] + "ves";

        if (lower.EndsWith("fe"))
            return noun[..^2] + "ves";

        if (lower.EndsWith('o') && noun.Length > 1 && !_vowels.Contains(noun[^2]))
            return noun + "es";

        return noun + "s";
    }

    public static string Singularize(string noun)
    {
        if (string.IsNullOrWhiteSpace(noun))
            return noun;

        if (_irregularSingulars.TryGetValue(noun, out string? irregular))
            return MatchCase(noun, irregular);

        string lower = noun.ToLowerInvariant();

        if (lower.EndsWith("ies") && noun.Length > 3)
            return noun[..^3] + "y";

        if (lower.EndsWith("ves"))
            return noun[..^3] + "f";

        if (lower.EndsWith("es") && noun.Length > 2)
        {
            string stem = noun[..^2];
            string stemLower = stem.ToLowerInvariant();

            if (stemLower.EndsWith('s') || stemLower.EndsWith('x') || stemLower.EndsWith('z')
                || stemLower.EndsWith("ch") || stemLower.EndsWith("sh"))
                return stem;
        }

        if (lower.EndsWith('s') && !lower.EndsWith("ss"))
            return noun[..^1];

        return noun;
    }

    public static string Quantify(string noun, int count, bool useArticleForOne = true)
    {
        return count switch
        {
            0 => $"no {Pluralize(noun)}",
            1 => useArticleForOne ? WithArticle(noun) : $"1 {noun}",
            _ => $"{count} {Pluralize(noun)}"
        };
    }

    public static string QuantifyWords(string noun, int count)
    {
        return count switch
        {
            0 => $"no {Pluralize(noun)}",
            1 => WithArticle(noun),
            >= 2 and <= 12 => $"{NumberToWords(count)} {Pluralize(noun)}",
            _ => $"{count} {Pluralize(noun)}"
        };
    }
    #endregion

    #region Possessives
    public static string Possessive(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return name;

        if (name.EndsWith('s') || name.EndsWith('S'))
            return name + "'";

        return name + "'s";
    }

    public static string PossessiveOf(string? owner, string item)
    {
        if (string.IsNullOrWhiteSpace(owner))
            return $"the {item}";

        return $"{Possessive(owner)} {item}";
    }
    #endregion

    #region Numbers and Ordinals
    private static readonly string[] _ones =
        [
            "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine",
            "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen",
            "seventeen", "eighteen", "nineteen"
        ];

    private static readonly string[] _tens =
        [
            "", "", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety"
        ];

    public static string NumberToWords(int number)
    {
        if (number < 0)
            return $"negative {NumberToWords(-number)}";

        if (number < 20)
            return _ones[number];

        if(number < 100)
        {
            int remainder = number % 10;
            return remainder == 0
                ? _tens[number / 10]
                : $"{_tens[number / 10]}-{_ones[remainder]}";
        }

        if(number < 1000)
        {
            int remainder = number % 100;
            return remainder == 0
                ? $"{_ones[number / 100]} hundred"
                : $"{_ones[number / 100]} hundred and {NumberToWords(remainder)}";
        }

        if(number < 1000000)
        {
            int remainder = number % 1000;
            return remainder == 0
                ? $"{_ones[number / 1000]} thousand"
                : $"{_ones[number / 1000]} thousand and {NumberToWords(remainder)}";
        }

        if(number < 1000000000)
        {
            int remainder = number % 1000000;
            return remainder == 0
                ? $"{_ones[number / 1000000]} million"
                : $"{_ones[number / 1000000]} million and {NumberToWords(remainder)}";
        }

        return number.ToString("N0");
    }

    public static string OrdinalSuffix(int number)
    {
        int abs = Math.Abs(number);
        int lastTwo = abs % 100;
        int lastOne = abs % 10;

        if (lastTwo >= 11 && lastTwo <= 13)
            return "th";

        return lastOne switch
        {
            1 => "st",
            2 => "nd",
            3 => "rd",
            _ => "th"
        };
    }

    public static string Ordinal(int number)
        => $"{number}{OrdinalSuffix(number)}";

    public static string OrdinalWords(int number)
    {
        if (number <= 0)
            return Ordinal(number);

        return number switch
        {
            1 => "first",
            2 => "second",
            3 => "third",
            4 => "fourth",
            5 => "fifth",
            6 => "sixth",
            7 => "seventh",
            8 => "eighth",
            9 => "ninth",
            10 => "tenth",
            11 => "eleventh",
            12 => "twelfth",
            _ => $"{NumberToWords(number)}{OrdinalSuffix(number)}"
        };
    }
    #endregion

    #region Lists
    public static string ToCommaList(this IEnumerable<string> items, bool oxfordComma = true)
    {
        List<string> list = [.. items];

        return list.Count switch
        {
            0 => string.Empty,
            1 => list[0],
            2 => $"{list[0]} and {list[1]}",
            _ => string.Concat(
                string.Join(", ", list.Take(list.Count - 1)),
                oxfordComma ? ", and " : " and ",
                list[^1])
        };
    }

    public static string ToCommaListOr(this IEnumerable<string> items, bool oxfordComma = true)
    {
        List<string> list = [.. items];

        return list.Count switch
        {
            0 => string.Empty,
            1 => list[0],
            2 => $"{list[0]} or {list[1]}",
            _ => string.Concat(
                string.Join(", ", list.Take(list.Count - 1)),
                oxfordComma ? ", or " : " or ",
                list[^1])
        };
    }

    public static string GroupAndDescribe(this IEnumerable<string> items)
    {
        var groups = items
            .GroupBy(i => i, StringComparer.OrdinalIgnoreCase)
            .Select(g => QuantifyWords(g.Key, g.Count()))
            .ToList();

        return groups.ToCommaList();
    }
    #endregion

    #region Capitalization
    public static string Capitalize(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        if (text.Length == 1)
            return text.ToUpperInvariant();

        return char.ToUpperInvariant(text[0]) + text[1..];
    }

    public static string CapitalizeSentences(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        StringBuilder sb = new(text.Length);
        bool capitalizeNext = true;

        foreach(char c in text)
        {
            if(capitalizeNext && char.IsLetter(c))
            {
                sb.Append(char.ToUpperInvariant(c));
                capitalizeNext = false;
            }
            else
            {
                sb.Append(c);

                if (c is '.' or '!' or '?')
                    capitalizeNext = true;
            }
        }

        return sb.ToString();
    }

    public static string ToTitleCase(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        StringBuilder sb = new(text.Length);
        bool capitalizeNext = true;

        foreach(char c in text)
        {
            if (char.IsWhiteSpace(c))
            {
                capitalizeNext = true;
                sb.Append(c);
            }
            else if (capitalizeNext)
            {
                sb.Append(char.ToUpperInvariant(c));
                capitalizeNext = false;
            }
            else
            {
                sb.Append(char.ToLowerInvariant(c));
            }
        }

        return sb.ToString();
    }
    #endregion

    #region Verb Helpers
    public static string IsAre(int count) => count == 1 ? "is" : "are";
    public static string HasHave(int count) => count == 1 ? "has" : "have";
    public static string WasWere(int count) => count == 1 ? "was" : "were";
    public static string ItThey(int count) => count == 1 ? "it" : "they";
    public static string ThisThese(int count) => count == 1 ? "this" : "these";
    public static string ThatThose(int count) => count == 1 ? "that" : "those";

    public static string ThirdPerson(string verb)
    {
        if (string.IsNullOrWhiteSpace(verb))
            return verb;

        string lower = verb.ToLowerInvariant();

        if (lower is "have")
            return MatchCase(verb, "has");

        if (lower is "be" or "am" or "are")
            return MatchCase(verb, "is");

        if (lower.EndsWith('s') || lower.EndsWith('x') || lower.EndsWith('z')
            || lower.EndsWith("ch") || lower.EndsWith("sh") || lower.EndsWith('o'))
            return verb + "es";

        if (lower.EndsWith('y') && verb.Length > 1 && !_vowels.Contains(verb[^2]))
            return verb[..^1] + "ies";

        return verb + "s";
    }
    #endregion

    #region Helpers
    private static string MatchCase(string source, string target)
    {
        if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target))
            return target;

        // All caps
        if (source.All(c => !char.IsLetter(c) || char.IsUpper(c)))
            return target.ToUpperInvariant();

        // First letter cap
        if (char.IsUpper(source[0]))
            return Capitalize(target);

        return target.ToLowerInvariant();
    }
    #endregion

    #region MUD-Specific Utilities

    #endregion
}

/*
*------------------------------------------------------------
* (Grammar.cs)
* See License.txt for licensing information.
*-----------------------------------------------------------
*/