using System.Collections.ObjectModel;

using dosymep.WPF.ViewModels;

namespace RevitAreaBoundaries.ViewModels;

internal class RevitElementGroupViewModel : BaseViewModel {
    private string _name;
    private ObservableCollection<RevitElementViewModel> _revitElementViewModels;
    
    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }
    
    public ObservableCollection<RevitElementViewModel> RevitElementViewModels {
        get => _revitElementViewModels;
        set => RaiseAndSetIfChanged(ref _revitElementViewModels, value);
    }
}
