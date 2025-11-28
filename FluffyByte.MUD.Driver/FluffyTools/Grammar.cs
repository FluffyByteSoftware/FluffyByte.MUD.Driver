/*
 * (Grammar.cs)
 *------------------------------------------------------------
 * Created - 11/25/2025 11:50:40 PM
 * Created by - Seliris
 *-------------------------------------------------------------
 */

using System.Text;

namespace FluffyByte.MUD.Driver.FluffyTools;

/// <summary>
/// Provides utility methods for English grammar operations, including article selection, pluralization, possessive
/// forms, ordinal formatting, list formatting, capitalization, and verb conjugation.
/// </summary>
/// <remarks>This class offers static methods to assist with common natural language tasks such as determining the
/// correct indefinite article ("a" or "an"), converting nouns between singular and plural forms (including handling
/// irregular cases), generating possessive phrases, formatting numbers and ordinals as words, constructing
/// comma-separated lists with proper conjunctions, and applying capitalization or title case. All methods are
/// thread-safe and can be used without instantiating the class. These utilities are useful for generating grammatically
/// correct text in applications such as games, reporting tools, or user interfaces.</remarks>
public static class Grammar
{
    #region Articles
    /// <summary>
    /// Represents the set of vowel characters used for article-related operations.
    /// </summary>
    /// <remarks>Includes both uppercase and lowercase English vowels. This set can be used to determine
    /// whether a character is a vowel in contexts such as text processing or linguistic analysis.</remarks>
    private static readonly HashSet<char> Vowels =
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
    /// <summary>
    /// Contains words that begin with a vowel letter but are pronounced with an initial consonant sound, requiring the
    /// article "a" instead of "an" in English grammar.
    /// </summary>
    /// <remarks>This collection is used to identify exceptions to standard article usage rules when
    /// determining whether to use "a" or "an" before a word. The set includes common words such as "one", "university",
    /// and "European".</remarks>
    private static readonly HashSet<string> AnExceptions =
        [
            "one", "once", "united", "unique", "unicorn", "union", "unit", "universe",
            "universal", "university", "uniform", "unison", "uranium", "urine", "usable",
            "usage", "use", "used", "useful", "user", "usual", "usually", "usurp",
            "utensil", "utility", "utopia", "european", "eulogy", "euphemism", "euphoria",
            "euthanasia", "ewe", "ewes"
        ];

    /// <summary>
    /// Provides a collection of English words that are exceptions to the standard rule for using the article 'an'
    /// before words starting with the letter 'h'.
    /// </summary>
    /// <remarks>This set includes words where the initial 'h' is silent, resulting in the use of 'an' instead
    /// of 'a' (e.g., 'an hour', 'an honest person'). The collection can be used to determine article usage in text
    /// processing or grammar correction scenarios.</remarks>
    private static readonly HashSet<string> AExceptions =
        [
            "heir", "honest", "honor", "honour", "hour", "hourly", "herb"
        ];

    /// <summary>
    /// Determines the appropriate English indefinite article ("a" or "an") for the specified word based on its initial
    /// sound and common exceptions.
    /// </summary>
    /// <remarks>This method accounts for common exceptions to standard article usage, such as words that
    /// begin with a vowel but use "a" due to pronunciation, and vice versa. It is case-insensitive and considers only
    /// the first word in the input string.</remarks>
    /// <param name="word">The word for which to determine the correct indefinite article. If the word is null, empty, or consists only of
    /// whitespace, "a" is returned.</param>
    /// <returns>A string containing either "a" or "an", representing the correct indefinite article for the specified word.</returns>
    /// <exception cref="NullReferenceException">Thrown if the first word extracted from <paramref name="word"/> is null.</exception>
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

            if (AnExceptions.Any(e => firstWord.StartsWith(e)))
                return "a";

            if (AExceptions.Any(e => firstWord.StartsWith(e)))
                return "an";

            return Vowels.Contains(word[0]) ? "an" : "a";
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

    /// <summary>
    /// Returns the specified word prefixed with its appropriate English article ('a' or 'an'), with the article
    /// capitalized.
    /// </summary>
    /// <remarks>The method determines the correct article ('a' or 'an') based on the initial sound of the
    /// provided word and capitalizes the article. If the input is null or empty, the behavior is undefined.</remarks>
    /// <param name="word">The word to be prefixed with a capitalized article. Cannot be null or empty.</param>
    /// <returns>A string containing the capitalized article followed by the specified word, separated by a space.</returns>
    public static string WithArticleCapitalized(string word)
        => $"{Capitalize(Article(word))} {word}";
    #endregion

    #region Pluralization
    /// <summary>
    /// Provides a mapping of singular English nouns to their irregular plural forms for use in pluralization logic.
    /// </summary>
    /// <remarks>This dictionary includes common irregular plurals for people, animals, fantasy creatures, and
    /// words of Latin or Greek origin. The mapping is case-insensitive, ensuring consistent pluralization regardless of
    /// input casing. Use this collection to look up the plural form of a singular noun when standard pluralization
    /// rules do not apply.</remarks>
    private static readonly Dictionary<string, string> IrregularPlurals = new(StringComparer.OrdinalIgnoreCase)
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

    /// <summary>
    /// Provides a mapping of irregular plural words to their singular forms.
    /// </summary>
    /// <remarks>This dictionary is used to handle exceptions to standard pluralization rules, enabling
    /// accurate conversion of irregular plurals to singular words. The keys represent plural forms, and the values
    /// represent their corresponding singular forms.</remarks>
    private static readonly Dictionary<string, string> IrregularSingulars;

    /// <summary>
    /// Initializes static data for the Grammar class, including mappings for irregular singular and plural forms.
    /// </summary>
    /// <remarks>This static constructor sets up internal dictionaries used for singularization and
    /// pluralization of words with irregular forms. It is invoked automatically before any static members of the
    /// Grammar class are accessed.</remarks>
    static Grammar()
    {
        IrregularSingulars = new(StringComparer.OrdinalIgnoreCase);

        foreach (var kvp in IrregularPlurals)
        {
            IrregularSingulars.TryAdd(kvp.Value, kvp.Key);
        }
    }

    /// <summary>
    /// Returns the plural form of the specified English noun according to common pluralization rules.
    /// </summary>
    /// <remarks>Handles standard English pluralization patterns, including some irregular nouns and common
    /// suffix rules. The method preserves the casing of the input noun. For nouns not covered by these rules, a simple
    /// 's' is appended.</remarks>
    /// <param name="noun">The singular noun to pluralize. Cannot be null or empty; leading and trailing whitespace is ignored.</param>
    /// <returns>A string containing the pluralized form of the input noun. If the input is null, empty, or consists only of
    /// whitespace, the original value is returned.</returns>
    public static string Pluralize(string noun)
    {
        if (string.IsNullOrWhiteSpace(noun))
            return noun;

        // Check irregular pronouns
        if (IrregularPlurals.TryGetValue(noun, out string? irregular))
            return MatchCase(noun, irregular);

        string lower = noun.ToLowerInvariant();

        if (lower.EndsWith('s') || lower.EndsWith('x') || lower.EndsWith('z')
            || lower.EndsWith("ch") || lower.EndsWith("sh"))
            return noun + "es";

        if (lower.EndsWith('y') && noun.Length > 1 && !Vowels.Contains(noun[^2]))
            return noun[..^1] + "ies";

        if (lower.EndsWith('f'))
            return noun[..^1] + "ves";

        if (lower.EndsWith("fe"))
            return noun[..^2] + "ves";

        if (lower.EndsWith('o') && noun.Length > 1 && !Vowels.Contains(noun[^2]))
            return noun + "es";

        return noun + "s";
    }

    /// <summary>
    /// Converts an English plural noun to its singular form.
    /// </summary>
    /// <remarks>Handles common English pluralization rules and some irregular nouns. The method preserves the
    /// casing of the input noun.</remarks>
    /// <param name="noun">The plural noun to be converted. Cannot be null or whitespace.</param>
    /// <returns>A string containing the singular form of the specified noun. If the input is not recognized as a plural noun,
    /// the original value is returned.</returns>
    public static string Singularize(string noun)
    {
        if (string.IsNullOrWhiteSpace(noun))
            return noun;

        if (IrregularSingulars.TryGetValue(noun, out string? irregular))
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

    /// <summary>
    /// Returns a string that combines a count with the specified noun, using correct pluralization and optional article
    /// for singular values.
    /// </summary>
    /// <remarks>The method assumes the noun is in singular form and applies basic English pluralization
    /// rules. The indefinite article is determined based on the noun's initial sound. This method does not handle
    /// irregular plural forms or non-English nouns.</remarks>
    /// <param name="noun">The noun to be quantified. This should be a singular English noun.</param>
    /// <param name="count">The number of items to quantify. Determines whether the noun is pluralized and which quantifier is used.</param>
    /// <param name="useArticleForOne">Indicates whether to use an indefinite article (such as "a" or "an") instead of "1" when the count is one. If
    /// <see langword="true"/>, an article is used; otherwise, "1" precedes the noun.</param>
    /// <returns>A string representing the quantified noun. For example, "no apples" for zero, "an apple" or "1 apple" for one,
    /// and "2 apples" for counts greater than one.</returns>
    public static string Quantify(string noun, int count, bool useArticleForOne = true)
    {
        return count switch
        {
            0 => $"no {Pluralize(noun)}",
            1 => useArticleForOne ? WithArticle(noun) : $"1 {noun}",
            _ => $"{count} {Pluralize(noun)}"
        };
    }

    /// <summary>
    /// Generates a human-readable phrase that quantifies the specified noun based on the provided count.
    /// </summary>
    /// <remarks>For counts between 2 and 12, the method uses the word form of the number (e.g., "three
    /// cats"). For counts greater than 12, the numeric value is used (e.g., "15 cats"). For a count of 1, an
    /// appropriate article is included (e.g., "a dog" or "an apple").</remarks>
    /// <param name="noun">The singular form of the noun to be quantified. Cannot be null or empty.</param>
    /// <param name="count">The number of items to quantify. Must be zero or greater.</param>
    /// <returns>A string representing the quantified noun, such as "no apples", "an apple", or "five apples", depending on the
    /// count.</returns>
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
    /// <summary>
    /// Returns the possessive form of the specified name according to standard English rules.
    /// </summary>
    /// <param name="name">The name to convert to its possessive form. If the name ends with 's' or 'S', only an apostrophe is added;
    /// otherwise, "'s" is appended.</param>
    /// <returns>A string containing the possessive form of the specified name. If <paramref name="name"/> is null, empty, or
    /// consists only of white-space characters, the original value is returned.</returns>
    public static string Possessive(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return name;

        if (name.EndsWith('s') || name.EndsWith('S'))
            return name + "'";

        return name + "'s";
    }

    /// <summary>
    /// Returns a possessive phrase describing ownership of an item, using the specified owner name if provided.
    /// </summary>
    /// <param name="owner">The name of the owner to use in the possessive phrase. If null, empty, or whitespace, a generic phrase is
    /// returned.</param>
    /// <param name="item">The name of the item being possessed.</param>
    /// <returns>A string representing the possessive form, such as "John's book" if an owner is specified, or "the book" if the
    /// owner is null or empty.</returns>
    public static string PossessiveOf(string? owner, string item)
    {
        if (string.IsNullOrWhiteSpace(owner))
            return $"the {item}";

        return $"{Possessive(owner)} {item}";
    }
    #endregion

    #region Numbers and Ordinals
    /// <summary>
    /// Provides the English word representations for the numbers zero through nineteen.
    /// </summary>
    private static readonly string[] Ones =
        [
            "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine",
            "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen",
            "seventeen", "eighteen", "nineteen"
        ];

    /// <summary>
    /// Provides the English word representations for the tens place values from twenty to ninety.
    /// </summary>
    /// <remarks>The array is indexed such that the value at index 2 corresponds to "twenty", index 3 to
    /// "thirty", and so on up to index 9 for "ninety". The first two elements are empty strings to align the indices
    /// with their numeric values.</remarks>
    private static readonly string[] Tens =
        [
            "", "", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety"
        ];

    /// <summary>
    /// Converts an integer value to its English words representation.
    /// </summary>
    /// <remarks>Supports numbers up to 999,999,999. Negative numbers are prefixed with "negative". For
    /// numbers greater than or equal to one billion, the numeric value is returned as a string.</remarks>
    /// <param name="number">The integer value to convert. Can be negative or positive.</param>
    /// <returns>A string containing the English words representation of the specified number. For values outside the supported
    /// range, the number is returned as a formatted string.</returns>
    public static string NumberToWords(int number)
    {
        if (number < 0)
            return $"negative {NumberToWords(-number)}";

        if (number < 20)
            return Ones[number];

        if(number < 100)
        {
            int remainder = number % 10;
            return remainder == 0
                ? Tens[number / 10]
                : $"{Tens[number / 10]}-{Ones[remainder]}";
        }

        if(number < 1000)
        {
            int remainder = number % 100;
            return remainder == 0
                ? $"{Ones[number / 100]} hundred"
                : $"{Ones[number / 100]} hundred and {NumberToWords(remainder)}";
        }

        if(number < 1000000)
        {
            int remainder = number % 1000;
            return remainder == 0
                ? $"{Ones[number / 1000]} thousand"
                : $"{Ones[number / 1000]} thousand and {NumberToWords(remainder)}";
        }

        if(number < 1000000000)
        {
            int remainder = number % 1000000;
            return remainder == 0
                ? $"{Ones[number / 1000000]} million"
                : $"{Ones[number / 1000000]} million and {NumberToWords(remainder)}";
        }

        return number.ToString("N0");
    }

    /// <summary>
    /// Returns the English ordinal suffix for the specified integer value.
    /// </summary>
    /// <remarks>The method follows standard English rules for ordinal suffixes. For numbers ending in 11, 12,
    /// or 13, the suffix "th" is returned regardless of the last digit.</remarks>
    /// <param name="number">The number for which to determine the ordinal suffix. Negative values are treated as their absolute value.</param>
    /// <returns>A string containing the ordinal suffix ("st", "nd", "rd", or "th") appropriate for the given number.</returns>
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

    /// <summary>
    /// Returns the ordinal string representation of the specified integer, appending the appropriate English ordinal
    /// suffix.
    /// </summary>
    /// <remarks>If the input number is negative, the result will include the negative sign and the ordinal
    /// suffix (e.g., "-1st"). This method does not perform localization and always uses English ordinal
    /// rules.</remarks>
    /// <param name="number">The integer value to convert to its ordinal string form. Must be a non-negative number.</param>
    /// <returns>A string containing the number followed by its English ordinal suffix (e.g., "1st", "2nd", "3rd").</returns>
    public static string Ordinal(int number)
        => $"{number}{OrdinalSuffix(number)}";

    /// <summary>
    /// Returns the English ordinal word representation of the specified number.
    /// </summary>
    /// <remarks>For numbers between 1 and 12, the method returns the standard English ordinal word (e.g.,
    /// "first", "twelfth"). For larger positive numbers, the result is constructed from the number's word form and its
    /// ordinal suffix (e.g., "twentieth").</remarks>
    /// <param name="number">The number to convert to its ordinal word form. Must be greater than zero to receive a word-based ordinal; zero
    /// or negative values return a numeric ordinal string.</param>
    /// <returns>A string containing the ordinal word for the specified number (e.g., "first", "twentieth"). For zero or negative
    /// values, returns a numeric ordinal string.</returns>
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
    /// <summary>
    /// Creates a human-readable, comma-separated list from a sequence of strings, using 'and' before the final item.
    /// </summary>
    /// <remarks>If the collection contains only one item, that item is returned without modification. For two
    /// items, they are joined with 'and' and no comma. For three or more items, items are separated by commas, and the
    /// Oxford comma is included or omitted based on the <paramref name="oxfordComma"/> parameter.</remarks>
    /// <param name="items">The collection of strings to be joined into a comma-separated list. Cannot be null.</param>
    /// <param name="oxfordComma">Specifies whether to include the Oxford comma before 'and' in lists of three or more items. Set to <see
    /// langword="true"/> to include the Oxford comma; otherwise, <see langword="false"/>.</param>
    /// <returns>A string containing the items joined in a comma-separated list with 'and' before the last item. Returns an empty
    /// string if the collection is empty.</returns>
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

    /// <summary>
    /// Creates a human-readable, comma-separated list from the specified collection of strings, using 'or' before the
    /// final item.
    /// </summary>
    /// <remarks>If the collection contains only one item, that item is returned without additional
    /// formatting. For two items, the result is formatted as 'A or B'. For three or more items, the result is formatted
    /// as 'A, B, or C' if <paramref name="oxfordComma"/> is <see langword="true"/>, or 'A, B or C' if <paramref
    /// name="oxfordComma"/> is <see langword="false"/>.</remarks>
    /// <param name="items">The collection of strings to format as a comma-separated list. If the collection is empty, an empty string is
    /// returned.</param>
    /// <param name="oxfordComma">Indicates whether to include the Oxford comma before 'or' in lists of three or more items. Set to <see
    /// langword="true"/> to include the Oxford comma; otherwise, <see langword="false"/>.</param>
    /// <returns>A formatted string representing the items as a comma-separated list with 'or' before the last item. Returns an
    /// empty string if the collection contains no items.</returns>
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

    /// <summary>
    /// Groups the specified collection of strings by value, ignoring case, and returns a comma-separated description of
    /// each group with its count.
    /// </summary>
    /// <remarks>String comparison is performed using ordinal case-insensitive matching. The output format is
    /// suitable for display or logging purposes.</remarks>
    /// <param name="items">The collection of strings to group and describe. Cannot be null.</param>
    /// <returns>A comma-separated string describing each unique value and its count in the input collection. Returns an empty
    /// string if the collection is empty.</returns>
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
    /// <summary>
    /// Returns a copy of the specified string with its first character converted to uppercase using the invariant
    /// culture.
    /// </summary>
    /// <param name="text">The string to capitalize. If null or empty, the original value is returned.</param>
    /// <returns>A string with the first character in uppercase if the input is not null or empty; otherwise, the original
    /// string.</returns>
    public static string Capitalize(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        if (text.Length == 1)
            return text.ToUpperInvariant();

        return char.ToUpperInvariant(text[0]) + text[1..];
    }

    /// <summary>
    /// Capitalizes the first letter of each sentence in the specified text.
    /// </summary>
    /// <remarks>Sentence boundaries are determined by the presence of '.', '!' or '?' characters. Only the
    /// first letter following these punctuation marks is capitalized; other letters remain unchanged.</remarks>
    /// <param name="text">The input string containing one or more sentences to be capitalized. Sentences are identified by ending
    /// punctuation marks such as '.', '!' or '?'.</param>
    /// <returns>A string in which the first letter of each sentence is capitalized. If <paramref name="text"/> is null or empty,
    /// the original value is returned.</returns>
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

    /// <summary>
    /// Converts the specified text to title case, capitalizing the first letter of each word and converting the
    /// remaining letters to lowercase.
    /// </summary>
    /// <remarks>Words are defined as sequences of characters separated by whitespace. The method does not
    /// account for locale-specific title casing rules and treats all non-whitespace characters as part of a
    /// word.</remarks>
    /// <param name="text">The input string to convert to title case. If <paramref name="text"/> is null or empty, the method returns the
    /// original value.</param>
    /// <returns>A string in title case, with the first character of each word capitalized and all other characters in lowercase.
    /// Returns the original value if <paramref name="text"/> is null or empty.</returns>
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
    /// <summary>
    /// Returns the appropriate verb form "is" or "are" based on the specified count.
    /// </summary>
    /// <param name="count">The number to evaluate. If equal to 1, "is" is returned; otherwise, "are" is returned.</param>
    /// <returns>A string containing "is" if <paramref name="count"/> equals 1; otherwise, "are".</returns>
    public static string IsAre(int count) => count == 1 ? "is" : "are";
    /// <summary>
    /// Returns the appropriate verb form "has" or "have" based on the specified count.
    /// </summary>
    /// <param name="count">The number to determine which verb form to use. If the value is 1, "has" is returned; otherwise, "have" is
    /// returned.</param>
    /// <returns>A string containing either "has" if <paramref name="count"/> is 1, or "have" for all other values.</returns>
    public static string HasHave(int count) => count == 1 ? "has" : "have";
    /// <summary>
    /// Returns the appropriate verb form "was" or "were" based on the specified count.
    /// </summary>
    /// <param name="count">The number to determine which verb form to use. If the value is 1, "was" is returned; otherwise, "were" is
    /// returned.</param>
    /// <returns>A string containing "was" if <paramref name="count"/> is 1; otherwise, "were".</returns>
    public static string WasWere(int count) => count == 1 ? "was" : "were";
    /// <summary>
    /// Returns the appropriate pronoun, "it" or "they", based on the specified count.
    /// </summary>
    /// <param name="count">The number of items to determine the pronoun. If equal to 1, "it" is returned; otherwise, "they" is returned.</param>
    /// <returns>A string containing "it" if <paramref name="count"/> is 1; otherwise, "they".</returns>
    public static string ItThey(int count) => count == 1 ? "it" : "they";
    /// <summary>
    /// Returns the appropriate demonstrative pronoun, "this" or "these", based on the specified count.
    /// </summary>
    /// <param name="count">The number of items to determine which pronoun to use. If the value is 1, "this" is returned; otherwise, "these"
    /// is returned.</param>
    /// <returns>A string containing "this" if <paramref name="count"/> is 1; otherwise, "these".</returns>
    public static string ThisThese(int count) => count == 1 ? "this" : "these";
    /// <summary>
    /// Returns the appropriate demonstrative pronoun, "that" or "those", based on the specified count.
    /// </summary>
    /// <param name="count">The number of items to determine the correct pronoun. If the value is 1, "that" is returned; otherwise, "those"
    /// is returned.</param>
    /// <returns>A string containing "that" if <paramref name="count"/> is 1; otherwise, "those".</returns>
    public static string ThatThose(int count) => count == 1 ? "that" : "those";

    /// <summary>
    /// Returns the third-person singular present tense form of the specified English verb.
    /// </summary>
    /// <remarks>The method applies standard English conjugation rules for regular verbs and handles common
    /// irregular cases such as "be" and "have". The casing of the returned verb matches the input verb.</remarks>
    /// <param name="verb">The base verb to conjugate. Cannot be null, but may be empty or whitespace.</param>
    /// <returns>A string containing the third-person singular present tense form of the input verb. If the input is null, empty,
    /// or whitespace, returns the input unchanged.</returns>
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

        if (lower.EndsWith('y') && verb.Length > 1 && !Vowels.Contains(verb[^2]))
            return verb[..^1] + "ies";

        return verb + "s";
    }
    #endregion

    #region Helpers
    /// <summary>
    /// Returns the target string with its casing adjusted to match the casing pattern of the source string.
    /// </summary>
    /// <param name="source">The string whose casing pattern is used as a reference. If all letters are uppercase, the target will be
    /// converted to uppercase; if only the first letter is uppercase, the target will be capitalized; otherwise, the
    /// target will be converted to lowercase.</param>
    /// <param name="target">The string to be transformed so that its casing matches the source string's pattern.</param>
    /// <returns>A string with the same casing pattern as the source string. If either parameter is null or empty, the target
    /// string is returned unchanged.</returns>
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
    /// <summary>
    /// Generates formatted action messages for a MUD scenario, providing distinct text for the actor, other observers,
    /// and the target (if any).
    /// </summary>
    /// <remarks>All returned messages are capitalized and formatted to include the actor, verbs, item, and
    /// target as appropriate. If <paramref name="target"/> is <see langword="null"/>, the target message will also be
    /// <see langword="null"/>.</remarks>
    /// <param name="actor">The name of the character performing the action. Used in messages shown to others and the target.</param>
    /// <param name="target">The name of the character who is the target of the action, or <see langword="null"/> if there is no target. If
    /// specified, a message will be generated for the target.</param>
    /// <param name="selfVerb">The verb describing the action from the actor's perspective (e.g., "pick up").</param>
    /// <param name="otherVerb">The verb describing the action from the perspective of other observers (e.g., "picks up").</param>
    /// <param name="item">The name of the item involved in the action, or <see langword="null"/> if no item is involved.</param>
    /// <returns>A tuple containing three strings: the message for the actor, the message for other observers, and the message
    /// for the target (or <see langword="null"/> if no target is specified).</returns>
    public static (string ToActor, string ToOthers, string? ToTarget) DescribeAction(
        string actor,
        string? target,
        string selfVerb,
        string otherVerb,
        string? item = null)
    {
        string itemPart = item != null ? $" {item}" : "";
        string targetPart = target != null ? $" {target}" : "";
        
        string toActor = $"You {selfVerb}{itemPart}{targetPart}.";
        string toOthers = $"{actor} {otherVerb}{itemPart}{targetPart}.";
        string? toTarget = target != null ? $"{actor} {otherVerb}{itemPart} you." : null;

        return (Capitalize(toActor), Capitalize(toOthers), toTarget != null ? Capitalize(toTarget) : null);
    }

    /// <summary>
    /// Wraps the input text into lines of a specified maximum width, breaking lines at word boundaries where possible.
    /// </summary>
    /// <remarks>Existing line breaks in the input are preserved. Words longer than the specified width are
    /// placed on their own line without splitting. Leading and trailing line breaks are trimmed from the
    /// result.</remarks>
    /// <param name="text">The text to be wrapped. May contain multiple lines separated by newline characters.</param>
    /// <param name="width">The maximum number of characters allowed per line. Must be greater than zero.</param>
    /// <returns>A string containing the word-wrapped text, with lines not exceeding the specified width. If the input is null,
    /// empty, or the width is less than or equal to zero, the original text is returned unchanged.</returns>
    public static string WordWrap(string text, int width = 80)
    {
        if(string.IsNullOrEmpty(text) || width <= 0)
        {
            return text;
        }

        StringBuilder sb = new();
        
        string[] lines = text.Split('\n');

        foreach(string line in lines)
        {
            if (line.Length <= width)
            {
                sb.AppendLine(line);
                continue;
            }

            string[] words = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            int currentLength = 0;

            foreach(string word in words)
            {
                if(currentLength + word.Length + 1 > width && currentLength > 0)
                {
                    sb.AppendLine();
                    currentLength = 0;
                }

                if(currentLength > 0)
                {
                    sb.Append(' ');
                    currentLength++;
                }

                sb.Append(word);
                currentLength += word.Length;
            }

            sb.AppendLine();
        }

        return sb.ToString().TrimEnd('\r', '\n');
    }
    #endregion
}

/*
*------------------------------------------------------------
* (Grammar.cs)
* See License.txt for licensing information.
*-----------------------------------------------------------
*/