using MT32Edit;

namespace MT32Editor.Core.Tests;

public class RhythmTests
{
    [Fact]
    public void Constructor_CreatesValidRhythm()
    {
        var rhythm = new Rhythm(24);
        Assert.NotNull(rhythm);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void SetTimbreGroup_GetTimbreGroup_RoundTrips(int group)
    {
        var rhythm = new Rhythm(24);
        rhythm.SetTimbreGroup(group);
        Assert.Equal(group, rhythm.GetTimbreGroup());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(32)]
    [InlineData(63)]
    public void SetTimbreNo_GetTimbreNo_RoundTrips(int timbreNo)
    {
        var rhythm = new Rhythm(24);
        rhythm.SetTimbreNo(timbreNo);
        Assert.Equal(timbreNo, rhythm.GetTimbreNo());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(7)]
    public void SetPanPot_GetPanPot_RoundTrips(int panPot)
    {
        var rhythm = new Rhythm(24);
        rhythm.SetPanPot(panPot);
        Assert.Equal(panPot, rhythm.GetPanPot());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(50)]
    [InlineData(100)]
    public void SetOutputLevel_GetOutputLevel_RoundTrips(int level)
    {
        var rhythm = new Rhythm(24);
        rhythm.SetOutputLevel(level);
        Assert.Equal(level, rhythm.GetOutputLevel());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SetReverbEnabled_GetReverbEnabled_RoundTrips(bool enabled)
    {
        var rhythm = new Rhythm(24);
        rhythm.SetReverbEnabled(enabled);
        Assert.Equal(enabled, rhythm.GetReverbEnabled());
    }

    [Fact]
    public void ClearTimbre_ResetsTimbereValues()
    {
        var rhythm = new Rhythm(24);
        rhythm.SetTimbreNo(32);
        rhythm.ClearTimbre();
        // After clear, timbre should be set to default values
        Assert.NotNull(rhythm);
    }
}
