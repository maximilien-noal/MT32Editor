using MT32Edit;
using Reqnroll;

namespace MT32Editor.BDD.Tests.StepDefinitions;

[Binding]
public class ApplicationSettingsSteps
{
    [Given("the default application state")]
    public void GivenTheDefaultApplicationState()
    {
        // Reset to known state
        SaveSysExFile.autoSave = true;
        LoadSysExFile.ignoreSystemArea = false;
        SaveSysExFile.excludeSystemArea = false;
        MT32SysEx.hardwareMT32Connected = true;
        MT32SysEx.sendTextToMT32 = true;
        MT32SysEx.allowReset = false;
    }

    // --- Autosave ---
    [When("I toggle autosave off")]
    public void WhenIToggleAutosaveOff()
    {
        SaveSysExFile.autoSave = false;
    }

    [When("I toggle autosave on")]
    public void WhenIToggleAutosaveOn()
    {
        SaveSysExFile.autoSave = true;
    }

    [Then("autosave should be disabled")]
    public void ThenAutosaveShouldBeDisabled()
    {
        Assert.False(SaveSysExFile.autoSave);
    }

    [Then("autosave should be enabled")]
    public void ThenAutosaveShouldBeEnabled()
    {
        Assert.True(SaveSysExFile.autoSave);
    }

    // --- Ignore system config on load ---
    [When("I enable ignore system config on load")]
    public void WhenIEnableIgnoreSysConfigOnLoad()
    {
        LoadSysExFile.ignoreSystemArea = true;
    }

    [When("I disable ignore system config on load")]
    public void WhenIDisableIgnoreSysConfigOnLoad()
    {
        LoadSysExFile.ignoreSystemArea = false;
    }

    [Then("ignore system config on load should be enabled")]
    public void ThenIgnoreSysConfigOnLoadShouldBeEnabled()
    {
        Assert.True(LoadSysExFile.ignoreSystemArea);
    }

    [Then("ignore system config on load should be disabled")]
    public void ThenIgnoreSysConfigOnLoadShouldBeDisabled()
    {
        Assert.False(LoadSysExFile.ignoreSystemArea);
    }

    // --- Exclude system config on save ---
    [When("I enable exclude system config on save")]
    public void WhenIEnableExcludeSysConfigOnSave()
    {
        SaveSysExFile.excludeSystemArea = true;
    }

    [When("I disable exclude system config on save")]
    public void WhenIDisableExcludeSysConfigOnSave()
    {
        SaveSysExFile.excludeSystemArea = false;
    }

    [Then("exclude system config on save should be enabled")]
    public void ThenExcludeSysConfigOnSaveShouldBeEnabled()
    {
        Assert.True(SaveSysExFile.excludeSystemArea);
    }

    [Then("exclude system config on save should be disabled")]
    public void ThenExcludeSysConfigOnSaveShouldBeDisabled()
    {
        Assert.False(SaveSysExFile.excludeSystemArea);
    }

    // --- Hardware MT-32 connected ---
    [When("I disconnect hardware MT-32")]
    public void WhenIDisconnectHardwareMT32()
    {
        MT32SysEx.hardwareMT32Connected = false;
    }

    [When("I connect hardware MT-32")]
    public void WhenIConnectHardwareMT32()
    {
        MT32SysEx.hardwareMT32Connected = true;
    }

    [Then("hardware MT-32 should be disconnected")]
    public void ThenHardwareMT32ShouldBeDisconnected()
    {
        Assert.False(MT32SysEx.hardwareMT32Connected);
    }

    [Then("hardware MT-32 should be connected")]
    public void ThenHardwareMT32ShouldBeConnected()
    {
        Assert.True(MT32SysEx.hardwareMT32Connected);
    }

    // --- Send messages to MT-32 ---
    [When("I disable send messages to MT-32")]
    public void WhenIDisableSendMessagesToMT32()
    {
        MT32SysEx.sendTextToMT32 = false;
    }

    [When("I enable send messages to MT-32")]
    public void WhenIEnableSendMessagesToMT32()
    {
        MT32SysEx.sendTextToMT32 = true;
    }

    [Then("send messages to MT-32 should be disabled")]
    public void ThenSendMessagesToMT32ShouldBeDisabled()
    {
        Assert.False(MT32SysEx.sendTextToMT32);
    }

    [Then("send messages to MT-32 should be enabled")]
    public void ThenSendMessagesToMT32ShouldBeEnabled()
    {
        Assert.True(MT32SysEx.sendTextToMT32);
    }

    // --- Allow MT-32 reset ---
    [When("I enable allow MT-32 reset")]
    public void WhenIEnableAllowMT32Reset()
    {
        MT32SysEx.allowReset = true;
    }

    [When("I disable allow MT-32 reset")]
    public void WhenIDisableAllowMT32Reset()
    {
        MT32SysEx.allowReset = false;
    }

    [Then("allow MT-32 reset should be enabled")]
    public void ThenAllowMT32ResetShouldBeEnabled()
    {
        Assert.True(MT32SysEx.allowReset);
    }

    [Then("allow MT-32 reset should be disabled")]
    public void ThenAllowMT32ResetShouldBeDisabled()
    {
        Assert.False(MT32SysEx.allowReset);
    }
}
