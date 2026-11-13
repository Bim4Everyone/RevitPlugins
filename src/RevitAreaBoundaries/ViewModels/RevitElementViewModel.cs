using dosymep.WPF.ViewModels;

using RevitAreaBoundaries.Models;

namespace RevitAreaBoundaries.ViewModels;

internal class RevitElementViewModel : BaseViewModel {
    private bool _isChecked;
    private string _name;

    public RevitElement RevitElement { get; set; }
    
    public bool IsChecked {
        get => _isChecked;
        set => RaiseAndSetIfChanged(ref _isChecked, value);
    }

    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }
}
