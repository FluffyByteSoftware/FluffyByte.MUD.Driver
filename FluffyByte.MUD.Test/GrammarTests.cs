/*
 * (GrammarTests.cs)
 *------------------------------------------------------------
 * Created - 11/26/2025
 * xUnit tests for Grammar.cs
 *-------------------------------------------------------------
 */

using FluffyByte.MUD.Driver.FluffyTools;
using Xunit;

namespace FluffyByte.MUD.Test;

public class GrammarTests
{
    #region Article Tests

    [Theory]
    [InlineData("sword", "a")]
    [InlineData("apple", "an")]
    [InlineData("elephant", "an")]
    [InlineData("umbrella", "an")]
    [InlineData("house", "a")]
    [InlineData("car", "a")]
    [InlineData("orange", "an")]
    [InlineData("ice cream", "an")]
    public void Article_BasicVowelConsonantRules_ReturnsCorrectArticle(string word, string expected)
    {
        var result = Grammar.Article(word);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("unicorn", "a")]
    [InlineData("unique", "a")]
    [InlineData("university", "a")]
    [InlineData("union", "a")]
    [InlineData("unit", "a")]
    [InlineData("one", "a")]
    [InlineData("once", "a")]
    [InlineData("european", "a")]
    [InlineData("eulogy", "a")]
    [InlineData("euphoria", "a")]
    [InlineData("ewe", "a")]
    public void Article_VowelsThatSoundLikeConsonants_ReturnsA(string word, string expected)
    {
        var result = Grammar.Article(word);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("heir", "an")]
    [InlineData("honest", "an")]
    [InlineData("honor", "an")]
    [InlineData("honour", "an")]
    [InlineData("hour", "an")]
    [InlineData("hourly", "an")]
    [InlineData("herb", "an")]
    public void Article_SilentHWords_ReturnsAn(string word, string expected)
    {
        var result = Grammar.Article(word);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Article_NullOrWhitespace_ReturnsA(string? word)
    {
        var result = Grammar.Article(word!);
        Assert.Equal("a", result);
    }

    [Theory]
    [InlineData("Apple", "an")]
    [InlineData("ELEPHANT", "an")]
    [InlineData("Unique", "a")]
    [InlineData("UNIVERSITY", "a")]
    public void Article_CaseSensitiveInput_ReturnsCorrectArticle(string word, string expected)
    {
        var result = Grammar.Article(word);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("big apple", "a")]
    [InlineData("old elephant", "an")]
    [InlineData("unique opportunity", "a")]
    public void Article_MultiWordPhrases_UsesFirstWordForDetermination(string phrase, string expected)
    {
        var result = Grammar.Article(phrase);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("sword", "a sword")]
    [InlineData("apple", "an apple")]
    [InlineData("unicorn", "a unicorn")]
    [InlineData("honest man", "an honest man")]
    public void WithArticle_ReturnsWordWithCorrectArticle(string word, string expected)
    {
        var result = Grammar.WithArticle(word);
        Assert.Equal(expected, result);
    }

    #endregion

    #region Pluralization Tests

    [Theory]
    [InlineData("sword", "swords")]
    [InlineData("cat", "cats")]
    [InlineData("dog", "dogs")]
    [InlineData("book", "books")]
    public void Pluralize_RegularNouns_AddsSuffix(string singular, string expected)
    {
        var result = Grammar.Pluralize(singular);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("box", "boxes")]
    [InlineData("bus", "buses")]
    [InlineData("buzz", "buzzes")]
    [InlineData("church", "churches")]
    [InlineData("wish", "wishes")]
    [InlineData("glass", "glasses")]
    public void Pluralize_SibilantEndings_AddsEs(string singular, string expected)
    {
        var result = Grammar.Pluralize(singular);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("baby", "babies")]
    [InlineData("city", "cities")]
    [InlineData("party", "parties")]
    [InlineData("fly", "flies")]
    public void Pluralize_ConsonantPlusY_ChangesToIes(string singular, string expected)
    {
        var result = Grammar.Pluralize(singular);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("boy", "boys")]
    [InlineData("key", "keys")]
    [InlineData("toy", "toys")]
    [InlineData("day", "days")]
    public void Pluralize_VowelPlusY_AddsS(string singular, string expected)
    {
        var result = Grammar.Pluralize(singular);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("hero", "heroes")]
    [InlineData("potato", "potatoes")]
    [InlineData("tomato", "tomatoes")]
    public void Pluralize_ConsonantPlusO_AddsEs(string singular, string expected)
    {
        var result = Grammar.Pluralize(singular);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("man", "men")]
    [InlineData("woman", "women")]
    [InlineData("child", "children")]
    [InlineData("person", "people")]
    [InlineData("foot", "feet")]
    [InlineData("tooth", "teeth")]
    [InlineData("goose", "geese")]
    [InlineData("mouse", "mice")]
    [InlineData("ox", "oxen")]
    public void Pluralize_IrregularNouns_ReturnsCorrectPlural(string singular, string expected)
    {
        var result = Grammar.Pluralize(singular);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("fish", "fish")]
    [InlineData("sheep", "sheep")]
    [InlineData("deer", "deer")]
    [InlineData("moose", "moose")]
    [InlineData("salmon", "salmon")]
    public void Pluralize_UnchangingNouns_ReturnsSameWord(string singular, string expected)
    {
        var result = Grammar.Pluralize(singular);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("dwarf", "dwarves")]
    [InlineData("elf", "elves")]
    [InlineData("wolf", "wolves")]
    [InlineData("knife", "knives")]
    [InlineData("leaf", "leaves")]
    [InlineData("thief", "thieves")]
    public void Pluralize_FantasyAndFWords_ReturnsVesForm(string singular, string expected)
    {
        var result = Grammar.Pluralize(singular);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("cactus", "cacti")]
    [InlineData("fungus", "fungi")]
    [InlineData("nucleus", "nuclei")]
    [InlineData("criterion", "criteria")]
    [InlineData("phenomenon", "phenomena")]
    [InlineData("datum", "data")]
    [InlineData("bacterium", "bacteria")]
    [InlineData("thesis", "theses")]
    [InlineData("analysis", "analyses")]
    [InlineData("matrix", "matrices")]
    public void Pluralize_LatinGreekNouns_ReturnsCorrectPlural(string singular, string expected)
    {
        var result = Grammar.Pluralize(singular);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Man", "Men")]
    [InlineData("CHILD", "CHILDREN")]
    [InlineData("Elf", "Elves")]
    public void Pluralize_PreservesCase(string singular, string expected)
    {
        var result = Grammar.Pluralize(singular);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Pluralize_NullOrWhitespace_ReturnsSameValue(string? noun)
    {
        var result = Grammar.Pluralize(noun!);
        Assert.Equal(noun, result);
    }

    #endregion

    #region Singularization Tests

    [Theory]
    [InlineData("swords", "sword")]
    [InlineData("cats", "cat")]
    [InlineData("dogs", "dog")]
    public void Singularize_RegularNouns_RemovesS(string plural, string expected)
    {
        var result = Grammar.Singularize(plural);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("boxes", "box")]
    [InlineData("buses", "bus")]
    [InlineData("churches", "church")]
    [InlineData("wishes", "wish")]
    public void Singularize_EsEndings_RemovesEs(string plural, string expected)
    {
        var result = Grammar.Singularize(plural);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("babies", "baby")]
    [InlineData("cities", "city")]
    [InlineData("parties", "party")]
    public void Singularize_IesEndings_ChangesToY(string plural, string expected)
    {
        var result = Grammar.Singularize(plural);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("knives", "knife")]
    [InlineData("wolves", "wolf")]
    [InlineData("leaves", "leaf")]
    public void Singularize_VesEndings_ChangesToF(string plural, string expected)
    {
        var result = Grammar.Singularize(plural);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("men", "man")]
    [InlineData("women", "woman")]
    [InlineData("children", "child")]
    [InlineData("people", "person")]
    [InlineData("feet", "foot")]
    [InlineData("teeth", "tooth")]
    [InlineData("geese", "goose")]
    [InlineData("mice", "mouse")]
    public void Singularize_IrregularNouns_ReturnsCorrectSingular(string plural, string expected)
    {
        var result = Grammar.Singularize(plural);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("fish", "fish")]
    [InlineData("sheep", "sheep")]
    [InlineData("deer", "deer")]
    public void Singularize_UnchangingNouns_ReturnsSameWord(string plural, string expected)
    {
        var result = Grammar.Singularize(plural);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Singularize_NullOrWhitespace_ReturnsSameValue(string? noun)
    {
        var result = Grammar.Singularize(noun!);
        Assert.Equal(noun, result);
    }

    #endregion

    #region Quantify Tests

    [Theory]
    [InlineData("sword", 0, true, "no swords")]
    [InlineData("apple", 0, true, "no apples")]
    [InlineData("child", 0, true, "no children")]
    public void Quantify_ZeroCount_ReturnsNoPlural(string noun, int count, bool useArticle, string expected)
    {
        var result = Grammar.Quantify(noun, count, useArticle);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("sword", 1, true, "a sword")]
    [InlineData("apple", 1, true, "an apple")]
    [InlineData("unicorn", 1, true, "a unicorn")]
    public void Quantify_OneWithArticle_ReturnsWithArticle(string noun, int count, bool useArticle, string expected)
    {
        var result = Grammar.Quantify(noun, count, useArticle);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("sword", 1, false, "1 sword")]
    [InlineData("apple", 1, false, "1 apple")]
    public void Quantify_OneWithoutArticle_ReturnsNumeral(string noun, int count, bool useArticle, string expected)
    {
        var result = Grammar.Quantify(noun, count, useArticle);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("sword", 5, true, "5 swords")]
    [InlineData("child", 3, true, "3 children")]
    [InlineData("box", 10, true, "10 boxes")]
    public void Quantify_MultipleCount_ReturnsCountWithPlural(string noun, int count, bool useArticle, string expected)
    {
        var result = Grammar.Quantify(noun, count, useArticle);
        Assert.Equal(expected, result);
    }

    #endregion

    #region QuantifyWords Tests

    [Theory]
    [InlineData("sword", 0, "no swords")]
    [InlineData("child", 0, "no children")]
    public void QuantifyWords_ZeroCount_ReturnsNoPlural(string noun, int count, string expected)
    {
        var result = Grammar.QuantifyWords(noun, count);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("sword", 1, "a sword")]
    [InlineData("apple", 1, "an apple")]
    public void QuantifyWords_OneCount_ReturnsWithArticle(string noun, int count, string expected)
    {
        var result = Grammar.QuantifyWords(noun, count);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("sword", 2, "two swords")]
    [InlineData("sword", 5, "five swords")]
    [InlineData("child", 3, "three children")]
    [InlineData("sword", 12, "twelve swords")]
    public void QuantifyWords_TwoToTwelve_ReturnsWordForm(string noun, int count, string expected)
    {
        var result = Grammar.QuantifyWords(noun, count);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("sword", 13, "13 swords")]
    [InlineData("sword", 100, "100 swords")]
    public void QuantifyWords_OverTwelve_ReturnsNumeral(string noun, int count, string expected)
    {
        var result = Grammar.QuantifyWords(noun, count);
        Assert.Equal(expected, result);
    }

    #endregion

    #region Possessive Tests

    [Theory]
    [InlineData("John", "John's")]
    [InlineData("Mary", "Mary's")]
    [InlineData("player", "player's")]
    public void Possessive_RegularNames_AddsApostropheS(string name, string expected)
    {
        var result = Grammar.Possessive(name);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("James", "James'")]
    [InlineData("Charles", "Charles'")]
    [InlineData("boss", "boss'")]
    [InlineData("BOSS", "BOSS'")]
    public void Possessive_NamesEndingInS_AddsOnlyApostrophe(string name, string expected)
    {
        var result = Grammar.Possessive(name);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Possessive_NullOrWhitespace_ReturnsSameValue(string? name)
    {
        var result = Grammar.Possessive(name!);
        Assert.Equal(name, result);
    }

    [Theory]
    [InlineData("John", "sword", "John's sword")]
    [InlineData("James", "shield", "James' shield")]
    [InlineData(null, "sword", "the sword")]
    [InlineData("", "sword", "the sword")]
    [InlineData("   ", "sword", "the sword")]
    public void PossessiveOf_ReturnsCorrectPossessivePhrase(string? owner, string item, string expected)
    {
        var result = Grammar.PossessiveOf(owner, item);
        Assert.Equal(expected, result);
    }

    #endregion

    #region NumberToWords Tests

    [Theory]
    [InlineData(0, "zero")]
    [InlineData(1, "one")]
    [InlineData(5, "five")]
    [InlineData(10, "ten")]
    [InlineData(13, "thirteen")]
    [InlineData(19, "nineteen")]
    public void NumberToWords_ZeroToNineteen_ReturnsCorrectWord(int number, string expected)
    {
        var result = Grammar.NumberToWords(number);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(20, "twenty")]
    [InlineData(30, "thirty")]
    [InlineData(50, "fifty")]
    [InlineData(90, "ninety")]
    public void NumberToWords_EvenTens_ReturnsCorrectWord(int number, string expected)
    {
        var result = Grammar.NumberToWords(number);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(21, "twenty-one")]
    [InlineData(45, "forty-five")]
    [InlineData(99, "ninety-nine")]
    public void NumberToWords_TwentyToNinetyNine_ReturnsHyphenatedForm(int number, string expected)
    {
        var result = Grammar.NumberToWords(number);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(100, "one hundred")]
    [InlineData(200, "two hundred")]
    [InlineData(500, "five hundred")]
    public void NumberToWords_EvenHundreds_ReturnsCorrectWord(int number, string expected)
    {
        var result = Grammar.NumberToWords(number);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(101, "one hundred and one")]
    [InlineData(256, "two hundred and fifty-six")]
    [InlineData(999, "nine hundred and ninety-nine")]
    public void NumberToWords_Hundreds_ReturnsCorrectPhrase(int number, string expected)
    {
        var result = Grammar.NumberToWords(number);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(1000, "one thousand")]
    [InlineData(5000, "five thousand")]
    public void NumberToWords_EvenThousands_ReturnsCorrectWord(int number, string expected)
    {
        var result = Grammar.NumberToWords(number);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(1001, "one thousand and one")]
    [InlineData(1234, "one thousand and two hundred and thirty-four")]
    public void NumberToWords_Thousands_ReturnsCorrectPhrase(int number, string expected)
    {
        var result = Grammar.NumberToWords(number);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(-1, "negative one")]
    [InlineData(-42, "negative forty-two")]
    [InlineData(-100, "negative one hundred")]
    public void NumberToWords_NegativeNumbers_ReturnsNegativePrefix(int number, string expected)
    {
        var result = Grammar.NumberToWords(number);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void NumberToWords_VeryLargeNumber_ReturnsFormattedNumber()
    {
        var result = Grammar.NumberToWords(1_000_000_000);
        Assert.Equal("1,000,000,000", result);
    }

    #endregion

    #region Ordinal Tests

    [Theory]
    [InlineData(1, "st")]
    [InlineData(2, "nd")]
    [InlineData(3, "rd")]
    [InlineData(4, "th")]
    [InlineData(5, "th")]
    [InlineData(10, "th")]
    public void OrdinalSuffix_BasicNumbers_ReturnsCorrectSuffix(int number, string expected)
    {
        var result = Grammar.OrdinalSuffix(number);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(11, "th")]
    [InlineData(12, "th")]
    [InlineData(13, "th")]
    public void OrdinalSuffix_ElevenToThirteen_ReturnsTh(int number, string expected)
    {
        var result = Grammar.OrdinalSuffix(number);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(21, "st")]
    [InlineData(22, "nd")]
    [InlineData(23, "rd")]
    [InlineData(24, "th")]
    [InlineData(31, "st")]
    [InlineData(42, "nd")]
    [InlineData(53, "rd")]
    public void OrdinalSuffix_HigherNumbers_ReturnsCorrectSuffix(int number, string expected)
    {
        var result = Grammar.OrdinalSuffix(number);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(111, "th")]
    [InlineData(112, "th")]
    [InlineData(113, "th")]
    public void OrdinalSuffix_OneHundredElevenToThirteen_ReturnsTh(int number, string expected)
    {
        var result = Grammar.OrdinalSuffix(number);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(1, "1st")]
    [InlineData(2, "2nd")]
    [InlineData(3, "3rd")]
    [InlineData(4, "4th")]
    [InlineData(11, "11th")]
    [InlineData(21, "21st")]
    public void Ordinal_ReturnsNumberWithSuffix(int number, string expected)
    {
        var result = Grammar.Ordinal(number);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(1, "first")]
    [InlineData(2, "second")]
    [InlineData(3, "third")]
    [InlineData(4, "fourth")]
    [InlineData(5, "fifth")]
    [InlineData(8, "eighth")]
    [InlineData(9, "ninth")]
    [InlineData(12, "twelfth")]
    public void OrdinalWords_OneToTwelve_ReturnsWordForm(int number, string expected)
    {
        var result = Grammar.OrdinalWords(number);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(13, "thirteenth")]
    [InlineData(20, "twentyth")]
    [InlineData(21, "twenty-onest")]
    public void OrdinalWords_OverTwelve_ReturnsNumberWordWithSuffix(int number, string expected)
    {
        var result = Grammar.OrdinalWords(number);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(0, "0th")]
    [InlineData(-1, "-1st")]
    public void OrdinalWords_ZeroOrNegative_ReturnsOrdinalNumeral(int number, string expected)
    {
        var result = Grammar.OrdinalWords(number);
        Assert.Equal(expected, result);
    }

    #endregion

    #region List Tests

    [Fact]
    public void ToCommaList_EmptyList_ReturnsEmptyString()
    {
        var items = Array.Empty<string>();
        var result = items.ToCommaList();
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ToCommaList_SingleItem_ReturnsSingleItem()
    {
        var items = new[] { "sword" };
        var result = items.ToCommaList();
        Assert.Equal("sword", result);
    }

    [Fact]
    public void ToCommaList_TwoItems_ReturnsWithAnd()
    {
        var items = new[] { "sword", "shield" };
        var result = items.ToCommaList();
        Assert.Equal("sword and shield", result);
    }

    [Fact]
    public void ToCommaList_ThreeItemsWithOxfordComma_ReturnsWithOxfordComma()
    {
        var items = new[] { "sword", "shield", "helm" };
        var result = items.ToCommaList(oxfordComma: true);
        Assert.Equal("sword, shield, and helm", result);
    }

    [Fact]
    public void ToCommaList_ThreeItemsWithoutOxfordComma_ReturnsWithoutOxfordComma()
    {
        var items = new[] { "sword", "shield", "helm" };
        var result = items.ToCommaList(oxfordComma: false);
        Assert.Equal("sword, shield and helm", result);
    }

    [Fact]
    public void ToCommaList_ManyItems_ReturnsCorrectFormat()
    {
        var items = new[] { "a", "b", "c", "d", "e" };
        var result = items.ToCommaList();
        Assert.Equal("a, b, c, d, and e", result);
    }

    [Fact]
    public void ToCommaListOr_TwoItems_ReturnsWithOr()
    {
        var items = new[] { "sword", "shield" };
        var result = items.ToCommaListOr();
        Assert.Equal("sword or shield", result);
    }

    [Fact]
    public void ToCommaListOr_ThreeItemsWithOxfordComma_ReturnsWithOxfordCommaAndOr()
    {
        var items = new[] { "sword", "shield", "helm" };
        var result = items.ToCommaListOr(oxfordComma: true);
        Assert.Equal("sword, shield, or helm", result);
    }

    [Fact]
    public void ToCommaListOr_ThreeItemsWithoutOxfordComma_ReturnsWithoutOxfordCommaAndOr()
    {
        var items = new[] { "sword", "shield", "helm" };
        var result = items.ToCommaListOr(oxfordComma: false);
        Assert.Equal("sword, shield or helm", result);
    }

    [Fact]
    public void GroupAndDescribe_SingleItems_ReturnsWithArticles()
    {
        var items = new[] { "sword", "shield", "helm" };
        var result = items.GroupAndDescribe();
        Assert.Equal("a sword, a shield, and a helm", result);
    }

    [Fact]
    public void GroupAndDescribe_DuplicateItems_ReturnsGroupedWithCounts()
    {
        var items = new[] { "sword", "sword", "shield", "sword" };
        var result = items.GroupAndDescribe();
        Assert.Equal("three swords and a shield", result);
    }

    [Fact]
    public void GroupAndDescribe_ManyDuplicates_ReturnsCorrectCounts()
    {
        var items = new[] { "gold coin", "gold coin", "gold coin", "gold coin", "gold coin", "silver coin", "silver coin" };
        var result = items.GroupAndDescribe();
        Assert.Equal("five gold coins and two silver coins", result);
    }

    #endregion

    #region Capitalization Tests

    [Theory]
    [InlineData("hello", "Hello")]
    [InlineData("world", "World")]
    [InlineData("a", "A")]
    public void Capitalize_LowercaseWord_CapitalizesFirstLetter(string input, string expected)
    {
        var result = Grammar.Capitalize(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Hello", "Hello")]
    [InlineData("HELLO", "HELLO")]
    public void Capitalize_AlreadyCapitalized_ReturnsSame(string input, string expected)
    {
        var result = Grammar.Capitalize(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Capitalize_NullOrEmpty_ReturnsSame(string? input)
    {
        var result = Grammar.Capitalize(input!);
        Assert.Equal(input, result);
    }

    [Theory]
    [InlineData("hello. world", "Hello. World")]
    [InlineData("hello! world? test.", "Hello! World? Test.")]
    [InlineData("first. second. third.", "First. Second. Third.")]
    public void CapitalizeSentences_MultipleSentences_CapitalizesEach(string input, string expected)
    {
        var result = Grammar.CapitalizeSentences(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void CapitalizeSentences_NullOrEmpty_ReturnsSame(string? input)
    {
        var result = Grammar.CapitalizeSentences(input!);
        Assert.Equal(input, result);
    }

    [Theory]
    [InlineData("hello world", "Hello World")]
    [InlineData("the quick brown fox", "The Quick Brown Fox")]
    [InlineData("HELLO WORLD", "Hello World")]
    public void ToTitleCase_ReturnsEachWordCapitalized(string input, string expected)
    {
        var result = Grammar.ToTitleCase(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void ToTitleCase_NullOrEmpty_ReturnsSame(string? input)
    {
        var result = Grammar.ToTitleCase(input!);
        Assert.Equal(input, result);
    }

    #endregion

    #region Verb Helper Tests

    [Theory]
    [InlineData(0, "are")]
    [InlineData(1, "is")]
    [InlineData(2, "are")]
    [InlineData(100, "are")]
    public void IsAre_ReturnsCorrectVerb(int count, string expected)
    {
        var result = Grammar.IsAre(count);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(0, "have")]
    [InlineData(1, "has")]
    [InlineData(2, "have")]
    public void HasHave_ReturnsCorrectVerb(int count, string expected)
    {
        var result = Grammar.HasHave(count);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(0, "were")]
    [InlineData(1, "was")]
    [InlineData(2, "were")]
    public void WasWere_ReturnsCorrectVerb(int count, string expected)
    {
        var result = Grammar.WasWere(count);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(0, "they")]
    [InlineData(1, "it")]
    [InlineData(2, "they")]
    public void ItThey_ReturnsCorrectPronoun(int count, string expected)
    {
        var result = Grammar.ItThey(count);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(0, "these")]
    [InlineData(1, "this")]
    [InlineData(2, "these")]
    public void ThisThese_ReturnsCorrectDeterminer(int count, string expected)
    {
        var result = Grammar.ThisThese(count);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(0, "those")]
    [InlineData(1, "that")]
    [InlineData(2, "those")]
    public void ThatThose_ReturnsCorrectDeterminer(int count, string expected)
    {
        var result = Grammar.ThatThose(count);
        Assert.Equal(expected, result);
    }

    #endregion

    #region ThirdPerson Tests

    [Theory]
    [InlineData("walk", "walks")]
    [InlineData("run", "runs")]
    [InlineData("jump", "jumps")]
    public void ThirdPerson_RegularVerbs_AddsS(string verb, string expected)
    {
        var result = Grammar.ThirdPerson(verb);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("have", "has")]
    [InlineData("Have", "Has")]
    [InlineData("HAVE", "HAS")]
    public void ThirdPerson_Have_ReturnsHas(string verb, string expected)
    {
        var result = Grammar.ThirdPerson(verb);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("be", "is")]
    [InlineData("am", "is")]
    [InlineData("are", "is")]
    [InlineData("Be", "Is")]
    public void ThirdPerson_Be_ReturnsIs(string verb, string expected)
    {
        var result = Grammar.ThirdPerson(verb);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("pass", "passes")]
    [InlineData("fix", "fixes")]
    [InlineData("buzz", "buzzes")]
    [InlineData("catch", "catches")]
    [InlineData("wash", "washes")]
    [InlineData("go", "goes")]
    [InlineData("do", "does")]
    public void ThirdPerson_SibilantEndings_AddsEs(string verb, string expected)
    {
        var result = Grammar.ThirdPerson(verb);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("try", "tries")]
    [InlineData("fly", "flies")]
    [InlineData("carry", "carries")]
    public void ThirdPerson_ConsonantPlusY_ChangesToIes(string verb, string expected)
    {
        var result = Grammar.ThirdPerson(verb);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("play", "plays")]
    [InlineData("say", "says")]
    [InlineData("stay", "stays")]
    public void ThirdPerson_VowelPlusY_AddsS(string verb, string expected)
    {
        var result = Grammar.ThirdPerson(verb);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void ThirdPerson_NullOrWhitespace_ReturnsSame(string? verb)
    {
        var result = Grammar.ThirdPerson(verb!);
        Assert.Equal(verb, result);
    }

    #endregion

    #region Edge Cases and Integration Tests

    [Fact]
    public void RoundTrip_PluralizeThenSingularize_IrregularNouns()
    {
        var irregulars = new[] { "man", "woman", "child", "mouse", "goose", "foot", "tooth" };

        foreach (var word in irregulars)
        {
            var plural = Grammar.Pluralize(word);
            var singular = Grammar.Singularize(plural);
            Assert.Equal(word, singular);
        }
    }

    [Fact]
    public void Integration_DescribeInventory_ProducesNaturalText()
    {
        var items = new[] { "sword", "shield", "potion", "potion", "potion", "gold coin", "gold coin" };
        var description = items.GroupAndDescribe();

        Assert.Contains("sword", description);
        Assert.Contains("shield", description);
        Assert.Contains("three potions", description);
        Assert.Contains("two gold coins", description);
    }

    [Fact]
    public void Integration_DescribeCombat_ProducesNaturalText()
    {
        int enemyCount = 3;
        string enemy = "goblin";

        var description = $"There {Grammar.IsAre(enemyCount)} {Grammar.Quantify(enemy, enemyCount)} ahead.";

        Assert.Equal("There are 3 goblins ahead.", description);
    }

    [Fact]
    public void Integration_DescribeSingleEnemy_ProducesNaturalText()
    {
        int enemyCount = 1;
        string enemy = "orc";

        var description = $"There {Grammar.IsAre(enemyCount)} {Grammar.Quantify(enemy, enemyCount)} ahead.";

        Assert.Equal("There is an orc ahead.", description);
    }

    #endregion
}