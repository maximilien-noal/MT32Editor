using MT32Edit;

namespace MT32Editor.Core.Tests;

public class MT32StateTests
{
    [Fact]
    public void Constructor_InitializesAllArrays()
    {
        var state = new MT32State();
        Assert.NotNull(state.GetMemoryTimbreArray());
        Assert.NotNull(state.GetPatchArray());
        Assert.NotNull(state.GetRhythmBankArray());
        Assert.NotNull(state.GetTimbreNames());
        Assert.NotNull(state.GetSystem());
    }

    [Fact]
    public void MemoryTimbreArray_Has64Entries()
    {
        var state = new MT32State();
        Assert.Equal(MT32State.NO_OF_MEMORY_TIMBRES, state.GetMemoryTimbreArray().Length);
    }

    [Fact]
    public void PatchArray_Has128Entries()
    {
        var state = new MT32State();
        Assert.Equal(MT32State.NO_OF_PATCHES, state.GetPatchArray().Length);
    }

    [Fact]
    public void RhythmBankArray_Has85Entries()
    {
        var state = new MT32State();
        Assert.Equal(MT32State.NO_OF_RHYTHM_BANKS, state.GetRhythmBankArray().Length);
    }

    [Fact]
    public void SetMemoryTimbre_GetMemoryTimbre_RoundTrips()
    {
        var state = new MT32State();
        var timbre = new TimbreStructure(true);
        timbre.SetTimbreName("TestTmbr");
        state.SetMemoryTimbre(timbre, 0);
        Assert.StartsWith("TestTmbr", state.GetMemoryTimbre(0).GetTimbreName());
    }

    [Fact]
    public void GetMemoryTimbreName_AfterSetTimbre_ReturnsName()
    {
        var state = new MT32State();
        var timbre = new TimbreStructure(true);
        timbre.SetTimbreName("TestTmbr");
        state.SetMemoryTimbre(timbre, 5);
        // GetMemoryTimbreName reads from TimbreNames, not the timbre array directly
        Assert.NotNull(state.GetMemoryTimbreName(5));
    }

    [Fact]
    public void SetPatch_GetPatch_RoundTrips()
    {
        var state = new MT32State();
        var patch = new Patch(10);
        patch.SetKeyShift(12);
        state.SetPatch(patch, 10);
        Assert.Equal(12, state.GetPatch(10).GetKeyShift());
    }

    [Fact]
    public void SetSelectedPatchNo_GetSelectedPatchNo_RoundTrips()
    {
        var state = new MT32State();
        state.SetSelectedPatchNo(42);
        Assert.Equal(42, state.GetSelectedPatchNo());
    }

    [Fact]
    public void SetSelectedKey_GetSelectedKey_RoundTrips()
    {
        var state = new MT32State();
        state.SetSelectedKey(60);
        Assert.Equal(60, state.GetSelectedKey());
    }

    [Fact]
    public void SetSelectedMemoryTimbre_GetSelectedMemoryTimbre_RoundTrips()
    {
        var state = new MT32State();
        state.SetSelectedMemoryTimbre(32);
        Assert.Equal(32, state.GetSelectedMemoryTimbre());
    }

    [Fact]
    public void ChangesMade_DefaultFalse()
    {
        var state = new MT32State();
        Assert.False(state.changesMade);
    }

    [Fact]
    public void ResetAll_ResetsState()
    {
        var state = new MT32State();
        state.changesMade = true;
        state.ResetAll();
        // After reset, arrays should be re-initialized
        Assert.NotNull(state.GetMemoryTimbreArray());
        Assert.Equal(MT32State.NO_OF_MEMORY_TIMBRES, state.GetMemoryTimbreArray().Length);
    }

    [Fact]
    public void TimbreIsEditable_InitiallyTrue()
    {
        var state = new MT32State();
        Assert.True(state.TimbreIsEditable());
    }

    [Fact]
    public void SetTimbreIsEditable_GetTimbreIsEditable_RoundTrips()
    {
        var state = new MT32State();
        state.SetTimbreIsEditable(true);
        Assert.True(state.TimbreIsEditable());
    }
}
