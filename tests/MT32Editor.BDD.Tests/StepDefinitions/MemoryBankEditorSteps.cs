using MT32Edit;
using Reqnroll;

namespace MT32Editor.BDD.Tests.StepDefinitions;

[Binding]
public class MemoryBankEditorSteps
{
    private MT32State _state = null!;
    private TimbreStructure _customTimbre = null!;

    [Given("a new MT-32 state is initialized")]
    public void GivenANewMT32StateIsInitialized()
    {
        _state = new MT32State();
    }

    [Then("the memory bank should have {int} timbre slots")]
    public void ThenTheMemoryBankShouldHaveTimbreSlots(int count)
    {
        Assert.Equal(count, _state.GetMemoryTimbreArray().Length);
    }

    [Then("each memory timbre should be accessible")]
    public void ThenEachMemoryTimbreShouldBeAccessible()
    {
        for (int i = 0; i < MT32State.NO_OF_MEMORY_TIMBRES; i++)
        {
            Assert.NotNull(_state.GetMemoryTimbre(i));
        }
    }

    [When("I create a custom timbre named {string}")]
    public void WhenICreateACustomTimbreNamed(string name)
    {
        _customTimbre = new TimbreStructure(createAudibleTimbre: true);
        _customTimbre.SetTimbreName(name);
    }

    [When("I store it in memory slot {int}")]
    public void WhenIStoreItInMemorySlot(int slot)
    {
        _state.SetMemoryTimbre(_customTimbre, slot);
    }

    [Then("memory slot {int} should contain the timbre")]
    public void ThenMemorySlotShouldContainTheTimbre(int slot)
    {
        Assert.NotNull(_state.GetMemoryTimbre(slot));
    }

    [Then("the timbre in slot {int} should be named {string}")]
    public void ThenTheTimbreInSlotShouldBeNamed(int slot, string name)
    {
        Assert.StartsWith(name, _state.GetMemoryTimbre(slot).GetTimbreName());
    }

    [When("I select memory timbre {int}")]
    public void WhenISelectMemoryTimbre(int timbre)
    {
        _state.SetSelectedMemoryTimbre(timbre);
    }

    [Then("the selected memory timbre should be {int}")]
    public void ThenTheSelectedMemoryTimbreShouldBe(int expected)
    {
        Assert.Equal(expected, _state.GetSelectedMemoryTimbre());
    }
}
