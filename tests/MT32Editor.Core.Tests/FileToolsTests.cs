using MT32Edit;

namespace MT32Editor.Core.Tests;

public class FileToolsTests
{
    [Theory]
    [InlineData("file.syx", true)]
    [InlineData("file.mid", true)]
    [InlineData("file.txt", false)]
    [InlineData("file.SYX", true)]
    [InlineData("file.MID", true)]
    [InlineData(null, false)]
    public void IsSysExOrMidi_ReturnsExpected(string? fileName, bool expected)
    {
        Assert.Equal(expected, FileTools.IsSysExOrMidi(fileName));
    }

    [Theory]
    [InlineData("valid", true)]
    [InlineData("Empty", false)]
    [InlineData("Error", false)]
    [InlineData("Cancelled", false)]
    public void Success_ReturnsExpected(string status, bool expected)
    {
        Assert.Equal(expected, FileTools.Success(status));
    }

    [Fact]
    public void RemoveInvalidFileNameCharacters_ReturnsCleanName()
    {
        var result = FileTools.RemoveInvalidFileNameCharacters("testfile");
        Assert.Equal("testfile", result);
    }

    [Fact]
    public void AppendNumber_AppendsCorrectly()
    {
        var result = FileTools.AppendNumber("test.syx", 2);
        Assert.Contains("2", result);
        Assert.EndsWith(".syx", result);
    }
}
