using dosymep.WPF.ViewModels;

namespace RevitLintelPlacement.ViewModels;

internal class LinkViewModel : BaseViewModel {
    private bool _isChecked;
    private string _name;

    public bool IsChecked {
        get => _isChecked;
        set => RaiseAndSetIfChanged(ref _isChecked, value);
    }

    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }
}
