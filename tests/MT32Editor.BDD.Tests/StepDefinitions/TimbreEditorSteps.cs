using MT32Edit;
using Reqnroll;

namespace MT32Editor.BDD.Tests.StepDefinitions;

[Binding]
public class TimbreEditorSteps
{
    private TimbreStructure _timbre = null!;
    private TimbreHistory _history = null!;
    private TimbreStructure _originalTimbre = null!;
    private byte[]? _copiedPartial;

    [Given("a new timbre editor is opened")]
    public void GivenANewTimbreEditorIsOpened()
    {
        _timbre = new TimbreStructure(createAudibleTimbre: true);
    }

    [Given("a new timbre editor is opened with history")]
    public void GivenANewTimbreEditorIsOpenedWithHistory()
    {
        _timbre = new TimbreStructure(createAudibleTimbre: true);
        _timbre.SetTimbreName("Original");
        _originalTimbre = _timbre.Clone();
        _history = new TimbreHistory(_timbre);
    }

    [When("I create a new audible timbre")]
    public void WhenICreateANewAudibleTimbre()
    {
        _timbre = new TimbreStructure(createAudibleTimbre: true);
    }

    [Then("the timbre should have a valid name")]
    public void ThenTheTimbreShouldHaveAValidName()
    {
        Assert.False(string.IsNullOrEmpty(_timbre.GetTimbreName()));
    }

    [Then("the timbre should have {int} partials")]
    public void ThenTheTimbreShouldHavePartials(int count)
    {
        Assert.Equal(count, TimbreConstants.NO_OF_PARTIALS);
    }

    [When("I set the timbre name to {string}")]
    public void WhenISetTheTimbreNameTo(string name)
    {
        _timbre.SetTimbreName(name);
    }

    [Then("the timbre name should start with {string}")]
    public void ThenTheTimbreNameShouldStartWith(string expected)
    {
        Assert.StartsWith(expected, _timbre.GetTimbreName());
    }

    [When("I select partial {int}")]
    public void WhenISelectPartial(int partial)
    {
        _timbre.SetActivePartial(partial);
    }

    [Then("the active partial should be {int}")]
    public void ThenTheActivePartialShouldBe(int expected)
    {
        Assert.Equal(expected, _timbre.GetActivePartial());
    }

    [When("I mute partial {int}")]
    public void WhenIMutePartial(int partial)
    {
        _timbre.SetPartialMuteStatus(partial, true);
    }

    [When("I unmute partial {int}")]
    public void WhenIUnmutePartial(int partial)
    {
        _timbre.SetPartialMuteStatus(partial, false);
    }

    [Then("partial {int} should be muted")]
    public void ThenPartialShouldBeMuted(int partial)
    {
        Assert.True(_timbre.GetPartialMuteStatus()[partial]);
    }

    [Then("partial {int} should not be muted")]
    public void ThenPartialShouldNotBeMuted(int partial)
    {
        Assert.False(_timbre.GetPartialMuteStatus()[partial]);
    }

    [When("I set structure {int} to {int}")]
    public void WhenISetStructureTo(int structurePair, int value)
    {
        if (structurePair == 12) _timbre.SetPart12Structure(value);
        else _timbre.SetPart34Structure(value);
    }

    // Feature file uses "1-2" but Reqnroll parses to separate ints; use regex
    [When(@"I set structure 1-2 to (\d+)")]
    public void WhenISetStructure12To(int value)
    {
        _timbre.SetPart12Structure(value);
    }

    [Then(@"structure 1-2 should be (\d+)")]
    public void ThenStructure12ShouldBe(int expected)
    {
        Assert.Equal(expected, _timbre.GetPart12Structure());
    }

    [When("I enable sustain")]
    public void WhenIEnableSustain()
    {
        _timbre.SetSustainStatus(true);
    }

    [When("I disable sustain")]
    public void WhenIDisableSustain()
    {
        _timbre.SetSustainStatus(false);
    }

    [Then("sustain should be enabled")]
    public void ThenSustainShouldBeEnabled()
    {
        Assert.True(_timbre.GetSustainStatus());
    }

    [Then("sustain should be disabled")]
    public void ThenSustainShouldBeDisabled()
    {
        Assert.False(_timbre.GetSustainStatus());
    }

    [When("I record the change in history")]
    public void WhenIRecordTheChangeInHistory()
    {
        _history.AddTo(_timbre.Clone());
    }

    [When("I undo the last change")]
    public void WhenIUndoTheLastChange()
    {
        _timbre = _history.Undo();
    }

    [When("I redo the last change")]
    public void WhenIRedoTheLastChange()
    {
        _timbre = _history.Redo();
    }

    [Then("the timbre should be restored to original state")]
    public void ThenTheTimbreShouldBeRestoredToOriginalState()
    {
        Assert.StartsWith("Original", _timbre.GetTimbreName());
    }

    [When("I set a parameter on partial {int}")]
    public void WhenISetAParameterOnPartial(int partial)
    {
        _timbre.SetUIParameter(partial, 0, 1);
    }

    [When("I copy partial {int}")]
    public void WhenICopyPartial(int partial)
    {
        _copiedPartial = _timbre.CopyPartial(partial);
    }

    [When("I paste to partial {int}")]
    public void WhenIPasteToPartial(int partial)
    {
        _timbre.PastePartial(partial, _copiedPartial!);
    }

    [Then("partial {int} should have the same parameter as partial {int}")]
    public void ThenPartialShouldHaveTheSameParameterAsPartial(int target, int source)
    {
        Assert.Equal(_timbre.GetUIParameter(source, 0), _timbre.GetUIParameter(target, 0));
    }
}
