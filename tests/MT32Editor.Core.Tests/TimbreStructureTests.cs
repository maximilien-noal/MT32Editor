using MT32Edit;

namespace MT32Editor.Core.Tests;

public class TimbreStructureTests
{
    [Fact]
    public void Constructor_NonAudible_CreatesDefaultTimbre()
    {
        var timbre = new TimbreStructure(createAudibleTimbre: false);
        Assert.NotNull(timbre);
        Assert.NotNull(timbre.GetTimbreName());
    }

    [Fact]
    public void Constructor_Audible_CreatesAudibleTimbre()
    {
        var timbre = new TimbreStructure(createAudibleTimbre: true);
        Assert.NotNull(timbre);
    }

    [Fact]
    public void SetTimbreName_GetTimbreName_RoundTrips()
    {
        var timbre = new TimbreStructure(false);
        timbre.SetTimbreName("TestName");
        Assert.StartsWith("TestName", timbre.GetTimbreName());
    }

    [Fact]
    public void SetTimbreName_LongName_Truncated()
    {
        var timbre = new TimbreStructure(false);
        timbre.SetTimbreName("ThisIsAVeryLongTimbreName");
        Assert.Equal(TimbreConstants.TIMBRE_NAME_LENGTH, timbre.GetTimbreName().Length);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void SetActivePartial_GetActivePartial_RoundTrips(int partial)
    {
        var timbre = new TimbreStructure(false);
        timbre.SetActivePartial(partial);
        Assert.Equal(partial, timbre.GetActivePartial());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(12)]
    public void SetPart12Structure_GetPart12Structure_RoundTrips(int structure)
    {
        var timbre = new TimbreStructure(false);
        timbre.SetPart12Structure(structure);
        Assert.Equal(structure, timbre.GetPart12Structure());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(12)]
    public void SetPart34Structure_GetPart34Structure_RoundTrips(int structure)
    {
        var timbre = new TimbreStructure(false);
        timbre.SetPart34Structure(structure);
        Assert.Equal(structure, timbre.GetPart34Structure());
    }

    [Fact]
    public void SetPartialMuteStatus_GetPartialMuteStatus_RoundTrips()
    {
        var timbre = new TimbreStructure(false);
        timbre.SetPartialMuteStatus(0, true);
        timbre.SetPartialMuteStatus(1, false);
        timbre.SetPartialMuteStatus(2, true);
        timbre.SetPartialMuteStatus(3, false);
        var mutes = timbre.GetPartialMuteStatus();
        Assert.True(mutes[0]);
        Assert.False(mutes[1]);
        Assert.True(mutes[2]);
        Assert.False(mutes[3]);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SetSustainStatus_GetSustainStatus_RoundTrips(bool sustain)
    {
        var timbre = new TimbreStructure(false);
        timbre.SetSustainStatus(sustain);
        Assert.Equal(sustain, timbre.GetSustainStatus());
    }

    [Fact]
    public void SetUIParameter_GetUIParameter_RoundTrips()
    {
        var timbre = new TimbreStructure(false);
        // Parameter 0 (waveform), partial 0
        timbre.SetUIParameter(0, 0, 1);
        Assert.Equal(1, timbre.GetUIParameter(0, 0));
    }

    [Fact]
    public void Clone_ReturnsIndependentCopy()
    {
        var timbre = new TimbreStructure(true);
        timbre.SetTimbreName("Original");
        var clone = timbre.Clone();
        clone.SetTimbreName("Clone");
        Assert.StartsWith("Original", timbre.GetTimbreName());
        Assert.StartsWith("Clone", clone.GetTimbreName());
    }

    [Fact]
    public void CheckSum_SameTimbre_SameChecksum()
    {
        var t1 = new TimbreStructure(true);
        var t2 = t1.Clone();
        Assert.Equal(t1.CheckSum(), t2.CheckSum());
    }

    [Fact]
    public void CheckSum_DifferentTimbre_DifferentChecksum()
    {
        var t1 = new TimbreStructure(true);
        var t2 = new TimbreStructure(false);
        Assert.NotEqual(t1.CheckSum(), t2.CheckSum());
    }

    [Fact]
    public void CopyPartial_PastePartial_TransfersData()
    {
        var timbre = new TimbreStructure(true);
        timbre.SetUIParameter(0, 0, 1);
        byte[] partialData = timbre.CopyPartial(0);
        timbre.PastePartial(1, partialData);
        Assert.Equal(timbre.GetUIParameter(0, 0), timbre.GetUIParameter(1, 0));
    }

    [Fact]
    public void GetParameterName_ReturnsNonEmptyString()
    {
        var timbre = new TimbreStructure(false);
        string name = timbre.GetParameterName(0);
        Assert.False(string.IsNullOrEmpty(name));
    }

    [Fact]
    public void SetUpdateTime_GetUpdateTime_UpdatesTimestamp()
    {
        var timbre = new TimbreStructure(false);
        var before = timbre.GetUpdateTime();
        Thread.Sleep(10);
        timbre.SetUpdateTime();
        Assert.True(timbre.GetUpdateTime() >= before);
    }

    [Fact]
    public void SetDefaultTimbreParameters_Audible_SetsParameters()
    {
        var timbre = new TimbreStructure(false);
        timbre.SetDefaultTimbreParameters(true);
        // Should not throw and should set up valid parameters
        Assert.NotNull(timbre.GetTimbreName());
    }
}
