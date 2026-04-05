using MT32Edit;

namespace MT32Editor.Core.Tests;

public class TimbreHistoryTests
{
    [Fact]
    public void Constructor_InitializesHistory()
    {
        var timbre = new TimbreStructure(true);
        var history = new TimbreHistory(timbre);
        Assert.Equal(0, history.GetLatestActionNo());
    }

    [Fact]
    public void AddTo_IncrementsActionNo()
    {
        var timbre = new TimbreStructure(true);
        var history = new TimbreHistory(timbre);
        var modified = timbre.Clone();
        modified.SetTimbreName("Modified");
        history.AddTo(modified);
        Assert.True(history.GetLatestActionNo() > 0);
    }

    [Fact]
    public void Undo_RestoresPreviousState()
    {
        var timbre = new TimbreStructure(true);
        timbre.SetTimbreName("Original");
        var history = new TimbreHistory(timbre);

        var modified = timbre.Clone();
        modified.SetTimbreName("Modified");
        history.AddTo(modified);

        var undone = history.Undo();
        Assert.StartsWith("Original", undone.GetTimbreName());
    }

    [Fact]
    public void Redo_RestoresNextState()
    {
        var timbre = new TimbreStructure(true);
        timbre.SetTimbreName("Original");
        var history = new TimbreHistory(timbre);

        var modified = timbre.Clone();
        modified.SetTimbreName("Modified");
        history.AddTo(modified);

        history.Undo();
        var redone = history.Redo();
        Assert.StartsWith("Modified", redone.GetTimbreName());
    }

    [Fact]
    public void Clear_ResetsHistory()
    {
        var timbre = new TimbreStructure(true);
        var history = new TimbreHistory(timbre);
        history.AddTo(timbre.Clone());
        history.AddTo(timbre.Clone());
        history.Clear(timbre);
        Assert.Equal(0, history.GetLatestActionNo());
    }

    [Fact]
    public void IsEqualTo_SameTimbre_ReturnsTrue()
    {
        var timbre = new TimbreStructure(true);
        var history = new TimbreHistory(timbre);
        Assert.True(history.IsEqualTo(timbre));
    }

    [Fact]
    public void IsDifferentTo_DifferentTimbre_ReturnsTrue()
    {
        var timbre = new TimbreStructure(true);
        var history = new TimbreHistory(timbre);
        var other = new TimbreStructure(false);
        Assert.True(history.IsDifferentTo(other));
    }
}
