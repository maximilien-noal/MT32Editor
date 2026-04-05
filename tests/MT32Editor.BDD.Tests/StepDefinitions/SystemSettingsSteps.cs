using MT32Edit;
using Reqnroll;

namespace MT32Editor.BDD.Tests.StepDefinitions;

[Binding]
public class SystemSettingsSteps
{
    private SystemLevel _system = null!;

    [Given("a new system level configuration")]
    public void GivenANewSystemLevelConfiguration()
    {
        _system = new SystemLevel();
    }

    [When("I set master volume to {int}")]
    public void WhenISetMasterVolumeTo(int level)
    {
        _system.SetMasterLevel(level);
    }

    [Then("master volume should be {int}")]
    public void ThenMasterVolumeShouldBe(int expected)
    {
        Assert.Equal(expected, _system.GetMasterLevel());
    }

    [When("I set master tune to {int}")]
    public void WhenISetMasterTuneTo(int tune)
    {
        _system.SetMasterTune(tune);
    }

    [Then("master tune should be {int}")]
    public void ThenMasterTuneShouldBe(int expected)
    {
        Assert.Equal(expected, _system.GetMasterTune());
    }

    [Then("master tune frequency should be displayed")]
    public void ThenMasterTuneFrequencyShouldBeDisplayed()
    {
        Assert.False(string.IsNullOrEmpty(_system.GetMasterTuneFrequency()));
    }

    [When("I set reverb mode to {int}")]
    public void WhenISetReverbModeTo(int mode)
    {
        _system.SetReverbMode(mode);
    }

    [When("I set reverb time to {int}")]
    public void WhenISetReverbTimeTo(int time)
    {
        _system.SetReverbTime(time);
    }

    [When("I set reverb level to {int}")]
    public void WhenISetReverbLevelTo(int level)
    {
        _system.SetReverbLevel(level);
    }

    [Then("reverb mode should be {int}")]
    public void ThenReverbModeShouldBe(int expected)
    {
        Assert.Equal(expected, _system.GetReverbMode());
    }

    [Then("reverb time should be {int}")]
    public void ThenReverbTimeShouldBe(int expected)
    {
        Assert.Equal(expected, _system.GetReverbTime());
    }

    [Then("reverb level should be {int}")]
    public void ThenReverbLevelShouldBe(int expected)
    {
        Assert.Equal(expected, _system.GetReverbLevel());
    }

    [When("I set MIDI channels 1 to 8")]
    public void WhenISetMIDIChannels1To8()
    {
        _system.SetMidiChannels1to8();
    }

    [When("I set MIDI channels 2 to 9")]
    public void WhenISetMIDIChannels2To9()
    {
        _system.SetMidiChannels2to9();
    }

    [Then("MIDI channels should be configured as 1 to 8")]
    public void ThenMIDIChannelsShouldBeConfiguredAs1To8()
    {
        Assert.True(_system.MidiChannelsAreSet1to8());
    }

    [Then("MIDI channels should be configured as 2 to 9")]
    public void ThenMIDIChannelsShouldBeConfiguredAs2To9()
    {
        Assert.True(_system.MidiChannelsAreSet2to9());
    }

    [When("I set partial reserve for part {int} to {int}")]
    public void WhenISetPartialReserveForPartTo(int part, int value)
    {
        _system.SetPartialReserve(part, value);
    }

    [Then("partial reserve for part {int} should be {int}")]
    public void ThenPartialReserveForPartShouldBe(int part, int expected)
    {
        Assert.Equal(expected, _system.GetPartialReserve(part));
    }

    [When("I set display message {int} to {string}")]
    public void WhenISetDisplayMessageTo(int messageNo, string text)
    {
        // BDD feature uses 1-based message numbers; API uses 0-based
        _system.SetMessage(messageNo - 1, text);
    }

    [Then("display message {int} should be {string}")]
    public void ThenDisplayMessageShouldBe(int messageNo, string expected)
    {
        // BDD feature uses 1-based message numbers; API uses 0-based
        string actual = _system.GetMessage(messageNo - 1);
        Assert.StartsWith(expected, actual);
    }

    [When("I set MIDI channel for part {int} to {int}")]
    public void WhenISetMIDIChannelForPartTo(int part, int channel)
    {
        _system.SetUIMidiChannel(part, channel);
    }

    [Then("MIDI channel for part {int} should be {int}")]
    public void ThenMIDIChannelForPartShouldBe(int part, int expected)
    {
        Assert.Equal(expected, _system.GetUIMidiChannel(part));
    }
}
