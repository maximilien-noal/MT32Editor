using MT32Edit;
using Reqnroll;

namespace MT32Editor.BDD.Tests.StepDefinitions;

[Binding]
public class RhythmEditorSteps
{
    private Rhythm _rhythm = null!;

    [Given("a new rhythm bank for key {int}")]
    public void GivenANewRhythmBankForKey(int key)
    {
        _rhythm = new Rhythm(key);
    }

    [Then("the rhythm bank should have valid defaults")]
    public void ThenTheRhythmBankShouldHaveValidDefaults()
    {
        Assert.NotNull(_rhythm);
        Assert.True(_rhythm.GetTimbreGroup() >= 0);
    }

    [When("I set the rhythm timbre group to {int}")]
    public void WhenISetTheRhythmTimbreGroupTo(int group)
    {
        _rhythm.SetTimbreGroup(group);
    }

    [When("I set the rhythm timbre number to {int}")]
    public void WhenISetTheRhythmTimbreNumberTo(int number)
    {
        _rhythm.SetTimbreNo(number);
    }

    [Then("the rhythm timbre group should be {int}")]
    public void ThenTheRhythmTimbreGroupShouldBe(int expected)
    {
        Assert.Equal(expected, _rhythm.GetTimbreGroup());
    }

    [Then("the rhythm timbre number should be {int}")]
    public void ThenTheRhythmTimbreNumberShouldBe(int expected)
    {
        Assert.Equal(expected, _rhythm.GetTimbreNo());
    }

    [When("I set the rhythm pan pot to {int}")]
    public void WhenISetTheRhythmPanPotTo(int value)
    {
        _rhythm.SetPanPot(value);
    }

    [Then("the rhythm pan pot should be {int}")]
    public void ThenTheRhythmPanPotShouldBe(int expected)
    {
        Assert.Equal(expected, _rhythm.GetPanPot());
    }

    [When("I set the rhythm output level to {int}")]
    public void WhenISetTheRhythmOutputLevelTo(int value)
    {
        _rhythm.SetOutputLevel(value);
    }

    [Then("the rhythm output level should be {int}")]
    public void ThenTheRhythmOutputLevelShouldBe(int expected)
    {
        Assert.Equal(expected, _rhythm.GetOutputLevel());
    }

    [When("I enable rhythm reverb")]
    public void WhenIEnableRhythmReverb()
    {
        _rhythm.SetReverbEnabled(true);
    }

    [When("I disable rhythm reverb")]
    public void WhenIDisableRhythmReverb()
    {
        _rhythm.SetReverbEnabled(false);
    }

    [Then("rhythm reverb should be enabled")]
    public void ThenRhythmReverbShouldBeEnabled()
    {
        Assert.True(_rhythm.GetReverbEnabled());
    }

    [Then("rhythm reverb should be disabled")]
    public void ThenRhythmReverbShouldBeDisabled()
    {
        Assert.False(_rhythm.GetReverbEnabled());
    }
}
