using MT32Edit;
using Reqnroll;

namespace MT32Editor.BDD.Tests.StepDefinitions;

[Binding]
public class PatchEditorSteps
{
    private Patch _patch = null!;

    [Given("a new patch is created for slot {int}")]
    public void GivenANewPatchIsCreatedForSlot(int slot)
    {
        _patch = new Patch(slot);
    }

    [Then("the patch should have valid default values")]
    public void ThenThePatchShouldHaveValidDefaultValues()
    {
        Assert.NotNull(_patch);
        Assert.True(_patch.GetTimbreGroup() >= 0);
        Assert.True(_patch.GetTimbreNo() >= 0);
    }

    [When("I set the timbre group to {int}")]
    public void WhenISetTheTimbreGroupTo(int group)
    {
        _patch.SetTimbreGroup(group);
    }

    [When("I set the timbre number to {int}")]
    public void WhenISetTheTimbreNumberTo(int number)
    {
        _patch.SetTimbreNo(number);
    }

    [Then("the timbre group should be {int}")]
    public void ThenTheTimbreGroupShouldBe(int expected)
    {
        Assert.Equal(expected, _patch.GetTimbreGroup());
    }

    [Then("the timbre group type should be {string}")]
    public void ThenTheTimbreGroupTypeShouldBe(string expected)
    {
        Assert.Equal(expected, _patch.GetTimbreGroupType());
    }

    [Then("the timbre number should be {int}")]
    public void ThenTheTimbreNumberShouldBe(int expected)
    {
        Assert.Equal(expected, _patch.GetTimbreNo());
    }

    [When("I set key shift to {int}")]
    public void WhenISetKeyShiftTo(int value)
    {
        _patch.SetKeyShift(value);
    }

    [When("I set fine tune to {int}")]
    public void WhenISetFineTuneTo(int value)
    {
        _patch.SetFineTune(value);
    }

    [When("I set bender range to {int}")]
    public void WhenISetBenderRangeTo(int value)
    {
        _patch.SetBenderRange(value);
    }

    [Then("key shift should be {int}")]
    public void ThenKeyShiftShouldBe(int expected)
    {
        Assert.Equal(expected, _patch.GetKeyShift());
    }

    [Then("fine tune should be {int}")]
    public void ThenFineTuneShouldBe(int expected)
    {
        Assert.Equal(expected, _patch.GetFineTune());
    }

    [Then("bender range should be {int}")]
    public void ThenBenderRangeShouldBe(int expected)
    {
        Assert.Equal(expected, _patch.GetBenderRange());
    }

    [When("I set assign mode to {int}")]
    public void WhenISetAssignModeTo(int value)
    {
        _patch.SetAssignMode(value);
    }

    [Then("assign mode should be {int}")]
    public void ThenAssignModeShouldBe(int expected)
    {
        Assert.Equal(expected, _patch.GetAssignMode());
    }

    [When("I enable patch reverb")]
    public void WhenIEnablePatchReverb()
    {
        _patch.SetReverbEnabled(true);
    }

    [When("I disable patch reverb")]
    public void WhenIDisablePatchReverb()
    {
        _patch.SetReverbEnabled(false);
    }

    [Then("patch reverb should be enabled")]
    public void ThenPatchReverbShouldBeEnabled()
    {
        Assert.True(_patch.GetReverbEnabled());
    }

    [Then("patch reverb should be disabled")]
    public void ThenPatchReverbShouldBeDisabled()
    {
        Assert.False(_patch.GetReverbEnabled());
    }

    [Then("the key shift should be {int}")]
    public void ThenTheKeyShiftShouldBe(int expected)
    {
        Assert.Equal(expected, _patch.GetKeyShift());
    }

    [Then("the fine tune should be {int}")]
    public void ThenTheFineTuneShouldBe(int expected)
    {
        Assert.Equal(expected, _patch.GetFineTune());
    }

    [Then("the bender range should be {int}")]
    public void ThenTheBenderRangeShouldBe(int expected)
    {
        Assert.Equal(expected, _patch.GetBenderRange());
    }
}
