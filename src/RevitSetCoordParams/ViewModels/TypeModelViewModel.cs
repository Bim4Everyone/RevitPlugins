using dosymep.WPF.ViewModels;

namespace RevitSetCoordParams.ViewModels;
internal class TypeModelViewModel : BaseViewModel {

    private string _name;

    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }
}
