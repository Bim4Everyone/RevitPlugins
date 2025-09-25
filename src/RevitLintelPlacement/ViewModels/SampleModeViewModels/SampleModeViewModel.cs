using dosymep.WPF.ViewModels;

using RevitLintelPlacement.Models.Interfaces;

namespace RevitLintelPlacement.ViewModels.SampleModeViewModels;

internal class SampleModeViewModel : BaseViewModel {
    private string _name;

    public SampleModeViewModel(
        string name,
        ILintelsProvider lintelsProvider,
        IElementsInWallProvider elementsInWallProvider) {
        Name = name;
        LintelsProvider = lintelsProvider;
        ElementsInWallProvider = elementsInWallProvider;
    }

    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }

    public ILintelsProvider LintelsProvider { get; }
    public IElementsInWallProvider ElementsInWallProvider { get; }
}
