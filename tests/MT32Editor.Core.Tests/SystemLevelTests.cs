using MT32Edit;

namespace MT32Editor.Core.Tests;

public class SystemLevelTests
{
    [Fact]
    public void Constructor_CreatesValidSystem()
    {
        var system = new SystemLevel();
        Assert.NotNull(system);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(50)]
    [InlineData(100)]
    public void SetMasterLevel_GetMasterLevel_RoundTrips(int level)
    {
        var system = new SystemLevel();
        system.SetMasterLevel(level);
        Assert.Equal(level, system.GetMasterLevel());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(64)]
    [InlineData(127)]
    public void SetMasterTune_GetMasterTune_RoundTrips(int tune)
    {
        var system = new SystemLevel();
        system.SetMasterTune(tune);
        Assert.Equal(tune, system.GetMasterTune());
    }

    [Fact]
    public void GetMasterTuneFrequency_ReturnsNonEmptyString()
    {
        var system = new SystemLevel();
        Assert.False(string.IsNullOrEmpty(system.GetMasterTuneFrequency()));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void SetReverbMode_GetReverbMode_RoundTrips(int mode)
    {
        var system = new SystemLevel();
        system.SetReverbMode(mode);
        Assert.Equal(mode, system.GetReverbMode());
    }

    [Fact]
    public void GetReverbModeName_ReturnsNonEmptyString()
    {
        var system = new SystemLevel();
        Assert.False(string.IsNullOrEmpty(system.GetReverbModeName()));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(7)]
    public void SetReverbTime_GetReverbTime_RoundTrips(int time)
    {
        var system = new SystemLevel();
        system.SetReverbTime(time);
        Assert.Equal(time, system.GetReverbTime());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(7)]
    public void SetReverbLevel_GetReverbLevel_RoundTrips(int level)
    {
        var system = new SystemLevel();
        system.SetReverbLevel(level);
        Assert.Equal(level, system.GetReverbLevel());
    }

    [Fact]
    public void GetReverbSysExValues_Returns3Bytes()
    {
        var system = new SystemLevel();
        var values = system.GetReverbSysExValues();
        Assert.Equal(3, values.Length);
    }

    [Fact]
    public void SetMidiChannels1to8_MidiChannelsAreSet1to8()
    {
        var system = new SystemLevel();
        system.SetMidiChannels1to8();
        Assert.True(system.MidiChannelsAreSet1to8());
    }

    [Fact]
    public void SetMidiChannels2to9_MidiChannelsAreSet2to9()
    {
        var system = new SystemLevel();
        system.SetMidiChannels2to9();
        Assert.True(system.MidiChannelsAreSet2to9());
    }

    [Fact]
    public void SetPartialReserve_GetPartialReserve_RoundTrips()
    {
        var system = new SystemLevel();
        system.SetPartialReserve(0, 8);
        Assert.Equal(8, system.GetPartialReserve(0));
    }

    [Fact]
    public void GetPartialReserveSysExValues_Returns9Bytes()
    {
        var system = new SystemLevel();
        var values = system.GetPartialReserveSysExValues();
        Assert.Equal(9, values.Length);
    }

    [Fact]
    public void SetMessage_GetMessage_RoundTrips()
    {
        var system = new SystemLevel();
        system.SetMessage(0, "Hello");
        Assert.Contains("Hello", system.GetMessage(0));
    }

    [Fact]
    public void GetMidiChannelSysExValues_Returns9Bytes()
    {
        var system = new SystemLevel();
        var values = system.GetMidiChannelSysExValues();
        Assert.Equal(9, values.Length);
    }
}
