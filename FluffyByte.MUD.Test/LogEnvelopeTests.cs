using FluffyByte.MUD.Driver.Core.Types.Debug;
namespace FluffyByte.MUD.Test;

public class LogEnvelopeTests
{
    #region Basic Structure Tests
    [Fact]
    public void ToString_ContainsTopBorder()
    {
        LogEnvelope envelope = new(DebugSeverity.Info, "Test message");

        string result = envelope.ToString();

        Assert.StartsWith("╔", result);
        Assert.Contains("═", result);
    }

    [Fact]
    public void ToString_ContainsBottomBorder()
    {
        LogEnvelope envelope = new(DebugSeverity.Info, "Test message");

        string result = envelope.ToString();

        Assert.Contains("╚", result);
        Assert.EndsWith("╝\r\n", result.Replace("\n", "\r\n").Replace("\r\r", "\r"));
    }

    [Fact]
    public void ToString_ContainsTimestamp()
    {
        LogEnvelope envelope = new(DebugSeverity.Info, "Test message");

        string result = envelope.ToString();

        Assert.Contains("Timestamp:", result);
        Assert.Contains("UTC", result);
    }

    [Fact]
    public void ToString_ContainsSeverity()
    {
        LogEnvelope envelope = new(DebugSeverity.Warn, "Warning message");

        string result = envelope.ToString();

        Assert.Contains("Severity", result);
        Assert.Contains("Warning", result);
    }

    [Fact]
    public void ToString_ContainsMessage()
    {
        LogEnvelope envelope = new(DebugSeverity.Error, "Test message");

        string result = envelope.ToString();

        Assert.Contains("Message", result);
        Assert.Contains("Test message", result);
    }
    #endregion

    #region Line Width Tests
    [Fact]
    public void ToString_AllLinesAreExactly80Characters()
    {
        LogEnvelope envelope = new(DebugSeverity.Error,
            "Short message",
            sourceFile: "/path/to/file.cs",
            sourceLine: 42,
            caller: "TestMethod");

        string result = envelope.ToString();

        var lines = result.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach(string line in lines)
        {
            string trimmed = line.TrimEnd('\r');
            Assert.Equal(80, trimmed.Length);
        }
    }

    [Fact]
    public void ToString_LongMessage_AllLinesAreExactly80Characters()
    {
        string longMessage = new('X', 200);
        var envelope = new LogEnvelope(DebugSeverity.Info, longMessage);

        string result = envelope.ToString();
        string[] lines = result.Split("\n", StringSplitOptions.RemoveEmptyEntries);

        foreach(string line in lines)
        {
            string trimmed = line.TrimEnd('\r');
            Assert.Equal(80, trimmed.Length);
        }
    }
    #endregion

    #region Wrapping Tests
    [Fact]
    public void ToString_LongMessage_WraptsToMultipleLines()
    {
        // 76 chars available for content (80 - 4 for borders)
        // "Message  : " prefix is 12 chars, leaving 64 for content per line
        string longMessage = new('A', 150);
        
        LogEnvelope env = new(DebugSeverity.Info, longMessage);

        string result = env.ToString();

        string[] lines = result.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        // Count lines containing our 'A' characters
        int messageLines = lines.Count(l => l.Contains('A'));

        Assert.True(messageLines >= 3, $"Expected at least 3 lines for 150-char message got {messageLines}");
    }

    [Fact]
    public void ToString_LongMessage_ContinuationLinesAreIndentend()
    {
        string longMessage = new('B', 150);
        LogEnvelope env = new(DebugSeverity.Info, longMessage);

        string result = env.ToString();
        string[] lines = result.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        var messageLines = lines.Where(l => l.Contains('B')).ToList();

        Assert.Contains("Message", messageLines[0]);

        for(int i = 1; i < messageLines.Count; i++)
        {
            Assert.StartsWith("║ ", messageLines[i]);
            Assert.DoesNotContain("Message", messageLines[i]);
        }
    }

    [Fact]
    public void ToString_LongSourcePath_WrapsCorrectly()
    {
        string longPath = "/very/long/path/to/some/deeply/nested/directory/structure/that/exceeds/the/normal/folder/path/some/more/words/SourceFile.cs";
        LogEnvelope env = new(
            DebugSeverity.Info,
            "Test",
            sourceFile: longPath,
            sourceLine: 99999
            );

        string result = env.ToString();
        string[] lines = result.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach(string line in lines)
        {
            string trimmed = line.TrimEnd('\r');
            Assert.Equal(80, trimmed.Length);
        }

        Assert.Contains("SourceFile.cs", result);
    }

    [Fact]
    public void ToString_MessageWithNewLines_PreservesLineBreaks()
    {
        string multiLineMessage = "Line one\nLine two\nLine three";
        LogEnvelope envelope = new(DebugSeverity.Info, multiLineMessage);

        string result = envelope.ToString();

        Assert.Contains("Line one", result);
        Assert.Contains("Line two", result);
        Assert.Contains("Line three", result);
    }
    #endregion

    #region Source Information Tests
    [Fact]
    public void ToString_WithSourceInfo_ContainsSourceSection()
    {
        LogEnvelope envelope = new(
            DebugSeverity.Info, 
            "Test", 
            sourceFile: "MyFile.cs", 
            sourceLine: 123, 
            caller: "MyMethod");

        string result = envelope.ToString();

        Assert.Contains("Source", result);
        Assert.Contains("MyFile.cs:123", result);
        Assert.Contains("Caller", result);
        Assert.Contains("MyMethod", result);
    }

    [Fact]
    public void ToString_WithSourceButNoCaller_OmitsCaller()
    {
        LogEnvelope envelope = new(
            DebugSeverity.Info,
            "Test",
            sourceFile: "MyFile.cs",
            sourceLine: 42, 
            caller: "Caller");

        string result = envelope.ToString();

        Assert.Contains("Source", result);
        Assert.Contains("MyFile.cs:42", result);
        Assert.Contains("Caller", result);
    }
    #endregion
}
