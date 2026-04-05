using MT32Edit;
using Reqnroll;

namespace MT32Editor.BDD.Tests.StepDefinitions;

[Binding]
public class TimbreCopyPasteSteps
{
    private TimbreStructure _timbre = null!;
    private byte[]? _clipboard;
    private bool _pasteAvailable;

    [Given("a timbre with modified partial {int}")]
    public void GivenATimbreWithModifiedPartial(int partial)
    {
        _timbre = new TimbreStructure(createAudibleTimbre: true);
        _timbre.SetUIParameter(partial, 0, 1); // Set waveform to PCM
        _timbre.SetUIParameter(partial, 0x01, 50); // Set fine pitch
    }

    [Given("a new timbre editor session")]
    public void GivenANewTimbreEditorSession()
    {
        _timbre = new TimbreStructure(createAudibleTimbre: true);
        _clipboard = null;
        _pasteAvailable = false;
    }

    [When("I copy partial {int} parameters")]
    public void WhenICopyPartialParameters(int partial)
    {
        _clipboard = _timbre.CopyPartial(partial);
        _pasteAvailable = _clipboard is not null;
    }

    [When("I paste partial {int} parameters to partial {int}")]
    public void WhenIPastePartialParametersToPartial(int source, int target)
    {
        if (_clipboard is not null)
        {
            _timbre.PastePartial(target, _clipboard);
        }
    }

    [Then("partial {int} should match partial {int} parameters")]
    public void ThenPartialShouldMatchPartialParameters(int target, int source)
    {
        Assert.Equal(_timbre.GetUIParameter(source, 0x01), _timbre.GetUIParameter(target, 0x01));
    }

    [Then("paste should be unavailable")]
    public void ThenPasteShouldBeUnavailable()
    {
        Assert.False(_pasteAvailable);
    }

    [Then("paste should be available")]
    public void ThenPasteShouldBeAvailable()
    {
        Assert.True(_pasteAvailable);
    }
}
