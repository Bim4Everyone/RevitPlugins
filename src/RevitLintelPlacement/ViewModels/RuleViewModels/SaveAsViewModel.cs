using dosymep.WPF.ViewModels;

namespace RevitLintelPlacement.ViewModels;

internal class SaveAsViewModel : BaseViewModel {
    private string _rulesFileName;

    public string RulesFileName {
        get => _rulesFileName;
        set => RaiseAndSetIfChanged(ref _rulesFileName, value);
    }
}
