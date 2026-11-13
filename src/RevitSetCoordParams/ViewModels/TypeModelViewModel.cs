using dosymep.WPF.ViewModels;

namespace RevitSetCoordParams.ViewModels;
internal class TypeModelViewModel : BaseViewModel {

    private string _name;
    private bool _isChecked;

    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }
    public bool IsChecked {
        get => _isChecked;
        set => RaiseAndSetIfChanged(ref _isChecked, value);
    }
}
