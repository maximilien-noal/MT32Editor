using MT32Edit;

namespace MT32Editor.Core.Tests;

public class ParseToolsTests
{
    [Theory]
    [InlineData("abc", 5, 5)]
    [InlineData("abcdef", 3, 3)]
    [InlineData("", 4, 4)]
    public void MakeNCharsLong_ReturnsCorrectLength(string input, int length, int expected)
    {
        Assert.Equal(expected, ParseTools.MakeNCharsLong(input, length).Length);
    }

    [Fact]
    public void PadWithSpace_PadsCorrectly()
    {
        var result = ParseTools.PadWithSpace("Hi", 5);
        Assert.Equal(5, result.Length);
        Assert.StartsWith("Hi", result);
    }

    [Theory]
    [InlineData("", true)]
    [InlineData("   ", true)]
    [InlineData(null, true)]
    [InlineData("text", false)]
    public void IsNullOrWhiteSpace_ReturnsExpected(string? input, bool expected)
    {
        Assert.Equal(expected, ParseTools.IsNullOrWhiteSpace(input!));
    }

    [Theory]
    [InlineData(1, "")]
    [InlineData(2, "s")]
    [InlineData(0, "s")]
    public void Pluralise_ReturnsExpected(int count, string expected)
    {
        Assert.Equal(expected, ParseTools.Pluralise(count));
    }

    [Theory]
    [InlineData(true, "*")]
    [InlineData(false, "")]
    public void UnsavedEdits_ReturnsExpected(bool changes, string expected)
    {
        Assert.Equal(expected, ParseTools.UnsavedEdits(changes));
    }

    [Theory]
    [InlineData("Hello World", 5, "Hello")]
    [InlineData("Hi", 10, "Hi")]
    public void TrimToLength_TrimsCorrectly(string input, int length, string expected)
    {
        Assert.Equal(expected, ParseTools.TrimToLength(input, length));
    }

    [Fact]
    public void RemoveTrailingSpaces_RemovesSpaces()
    {
        Assert.Equal("Hello", ParseTools.RemoveTrailingSpaces("Hello   "));
    }

    [Fact]
    public void RemoveLeadingSpaces_RemovesSpaces()
    {
        Assert.Equal("Hello", ParseTools.RemoveLeadingSpaces("   Hello"));
    }

    [Fact]
    public void LeftMost_ReturnsLeftChars()
    {
        Assert.Equal("Hel", ParseTools.LeftMost("Hello", 3));
    }

    [Fact]
    public void RightMost_ReturnsRightChars()
    {
        Assert.Equal("llo", ParseTools.RightMost("Hello", 3));
    }

    [Fact]
    public void RightOfChar_ReturnsCorrectSubstring()
    {
        Assert.Equal("World", ParseTools.RightOfChar("Hello.World", '.'));
    }

    [Fact]
    public void LeftOfChar_ReturnsCorrectSubstring()
    {
        Assert.Equal("Hello", ParseTools.LeftOfChar("Hello.World", '.'));
    }

    [Theory]
    [InlineData("True", true)]
    [InlineData("False", false)]
    [InlineData("invalid", null)]
    public void StringToBool_ReturnsExpected(string input, bool? expected)
    {
        Assert.Equal(expected, ParseTools.StringToBool(input));
    }

    [Fact]
    public void Architecture_ReturnsNonEmptyString()
    {
        Assert.False(string.IsNullOrEmpty(ParseTools.Architecture()));
    }

    [Fact]
    public void GetVersion_ReturnsFormattedVersion()
    {
        var result = ParseTools.GetVersion("v1.0");
        Assert.False(string.IsNullOrEmpty(result));
    }
}
