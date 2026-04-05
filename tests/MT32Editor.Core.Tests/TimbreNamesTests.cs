using MT32Edit;

namespace MT32Editor.Core.Tests;

public class TimbreNamesTests
{
    [Fact]
    public void Get_ReturnsNonNullName()
    {
        var state = new MT32State();
        var names = state.GetTimbreNames();
        // Group 0 = Preset A, should have names
        Assert.NotNull(names.Get(0, 0));
    }

    [Fact]
    public void GetAll_Group0_Returns64Names()
    {
        var state = new MT32State();
        var names = state.GetTimbreNames();
        var all = names.GetAll(0);
        Assert.Equal(TimbreConstants.NO_OF_TIMBRES_PER_GROUP, all.Length);
    }

    [Fact]
    public void GetAllWithIndices_Group0_Returns64Names()
    {
        var state = new MT32State();
        var names = state.GetTimbreNames();
        var all = names.GetAllWithIndices(0);
        Assert.Equal(TimbreConstants.NO_OF_TIMBRES_PER_GROUP, all.Length);
    }

    [Fact]
    public void SetMemoryTimbreName_GetMemoryTimbreNames_RoundTrips()
    {
        var state = new MT32State();
        var names = state.GetTimbreNames();
        names.SetMemoryTimbreName("TestName", 0);
        var memNames = names.GetAllMemoryTimbreNames();
        Assert.StartsWith("TestName", memNames[0]);
    }

    [Fact]
    public void ResetMemoryTimbreName_ResetsToDefault()
    {
        var state = new MT32State();
        var names = state.GetTimbreNames();
        names.SetMemoryTimbreName("Custom", 0);
        names.ResetMemoryTimbreName(0);
        var memNames = names.GetAllMemoryTimbreNames();
        Assert.DoesNotContain("Custom", memNames[0]);
    }

    [Fact]
    public void ResetAllMemoryTimbreNames_ResetsAll()
    {
        var state = new MT32State();
        var names = state.GetTimbreNames();
        names.SetMemoryTimbreName("Custom", 0);
        names.SetMemoryTimbreName("Custom2", 1);
        names.ResetAllMemoryTimbreNames();
        var memNames = names.GetAllMemoryTimbreNames();
        Assert.DoesNotContain("Custom", memNames[0]);
    }
}
