using MT32Edit;

namespace MT32Editor.Core.Tests;

public class PatchTests
{
    [Fact]
    public void Constructor_CreatesValidPatch()
    {
        var patch = new Patch(0);
        Assert.NotNull(patch);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(63)]
    [InlineData(127)]
    public void Constructor_ValidPatchNumbers(int patchNo)
    {
        var patch = new Patch(patchNo);
        Assert.NotNull(patch);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void SetTimbreGroup_GetTimbreGroup_RoundTrips(int group)
    {
        var patch = new Patch(0);
        patch.SetTimbreGroup(group);
        Assert.Equal(group, patch.GetTimbreGroup());
    }

    [Theory]
    [InlineData(0, "Preset A")]
    [InlineData(1, "Preset B")]
    [InlineData(2, "Memory")]
    [InlineData(3, "Rhythm")]
    public void GetTimbreGroupType_ReturnsCorrectType(int group, string expected)
    {
        var patch = new Patch(0);
        patch.SetTimbreGroup(group);
        Assert.Equal(expected, patch.GetTimbreGroupType());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(32)]
    [InlineData(63)]
    public void SetTimbreNo_GetTimbreNo_RoundTrips(int timbreNo)
    {
        var patch = new Patch(0);
        patch.SetTimbreNo(timbreNo);
        Assert.Equal(timbreNo, patch.GetTimbreNo());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(12)]
    [InlineData(24)]
    public void SetKeyShift_GetKeyShift_RoundTrips(int shift)
    {
        var patch = new Patch(0);
        patch.SetKeyShift(shift);
        Assert.Equal(shift, patch.GetKeyShift());
    }

    [Theory]
    [InlineData(-50)]
    [InlineData(0)]
    [InlineData(50)]
    public void SetFineTune_GetFineTune_RoundTrips(int tune)
    {
        var patch = new Patch(0);
        patch.SetFineTune(tune);
        Assert.Equal(tune, patch.GetFineTune());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(12)]
    [InlineData(24)]
    public void SetBenderRange_GetBenderRange_RoundTrips(int range)
    {
        var patch = new Patch(0);
        patch.SetBenderRange(range);
        Assert.Equal(range, patch.GetBenderRange());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void SetAssignMode_GetAssignMode_RoundTrips(int mode)
    {
        var patch = new Patch(0);
        patch.SetAssignMode(mode);
        Assert.Equal(mode, patch.GetAssignMode());
    }

    [Fact]
    public void GetAssignModeType_ReturnsNonEmptyString()
    {
        var patch = new Patch(0);
        Assert.False(string.IsNullOrEmpty(patch.GetAssignModeType()));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SetReverbEnabled_GetReverbEnabled_RoundTrips(bool enabled)
    {
        var patch = new Patch(0);
        patch.SetReverbEnabled(enabled);
        Assert.Equal(enabled, patch.GetReverbEnabled());
    }
}
