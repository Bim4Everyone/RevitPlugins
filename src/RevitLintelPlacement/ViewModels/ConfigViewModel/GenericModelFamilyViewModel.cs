using dosymep.WPF.ViewModels;

namespace RevitLintelPlacement.ViewModels;

internal class GenericModelFamilyViewModel : BaseViewModel {
    private bool _isChecked;
    private string name;

    public bool IsChecked {
        get => _isChecked;
        set => RaiseAndSetIfChanged(ref _isChecked, value);
    }

    public string Name {
        get => name;
        set => RaiseAndSetIfChanged(ref name, value);
    }
}
