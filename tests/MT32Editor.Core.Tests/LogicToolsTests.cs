using MT32Edit;

namespace MT32Editor.Core.Tests;

public class LogicToolsTests
{
    [Theory]
    [InlineData(true, 1)]
    [InlineData(false, 0)]
    public void BoolToInt_ConvertsCorrectly(bool input, int expected)
    {
        Assert.Equal(expected, LogicTools.BoolToInt(input));
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(2, true)]
    public void IntToBool_ConvertsCorrectly(int input, bool expected)
    {
        Assert.Equal(expected, LogicTools.IntToBool(input));
    }

    [Theory]
    [InlineData(10, 5, true)]
    [InlineData(10, 3, false)]
    [InlineData(0, 5, true)]
    public void DivisibleBy_ReturnsCorrectResult(int value, int divisor, bool expected)
    {
        Assert.Equal(expected, LogicTools.DivisibleBy(value, divisor));
    }

    [Theory]
    [InlineData(50, 0, 100, 50)]
    [InlineData(-1, 0, 100, 0)]
    [InlineData(101, 0, 100, 100)]
    public void AutoCorrect_ClampsValue(int value, int min, int max, int expected)
    {
        Assert.Equal(expected, LogicTools.AutoCorrect(value, min, max));
    }

    [Theory]
    [InlineData(50, 0, 100, true, 50)]
    [InlineData(-1, 0, 100, true, 0)]
    [InlineData(101, 0, 100, true, 100)]
    public void ValidateRange_WithAutoCorrect_ClampsValue(int value, int min, int max, bool autoCorrect, int expected)
    {
        Assert.Equal(expected, LogicTools.ValidateRange("test", value, min, max, autoCorrect));
    }

    [Fact]
    public void ValidateRange_WithoutAutoCorrect_InRange_ReturnsValue()
    {
        Assert.Equal(50, LogicTools.ValidateRange("test", 50, 0, 100, false));
    }

    [Theory]
    [InlineData(true, false, false, false, 0)]
    [InlineData(false, true, false, false, 1)]
    [InlineData(false, false, true, false, 2)]
    [InlineData(false, false, false, true, 3)]
    public void GetRadioButtonValue_ReturnsCorrectIndex(bool a, bool b, bool c, bool d, int expected)
    {
        Assert.Equal(expected, LogicTools.GetRadioButtonValue(a, b, c, d));
    }
}
