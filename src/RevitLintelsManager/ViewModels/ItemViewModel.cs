using dosymep.WPF.ViewModels;

namespace RevitLintelsManager.ViewModels;

internal class ItemViewModel : BaseViewModel {
    private string _name;
    
    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }
}
