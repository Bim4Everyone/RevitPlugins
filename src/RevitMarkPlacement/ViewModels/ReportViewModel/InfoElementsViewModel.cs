using System.Collections.ObjectModel;

using dosymep.WPF.ViewModels;

namespace RevitMarkPlacement.ViewModels;

internal class InfoElementsViewModel : BaseViewModel {
    private ObservableCollection<InfoElementViewModel> _infoElements;

    public InfoElementsViewModel() {
        InfoElements = new ObservableCollection<InfoElementViewModel>();
    }

    public ObservableCollection<InfoElementViewModel> InfoElements {
        get => _infoElements;
        set => RaiseAndSetIfChanged(ref _infoElements, value);
    }
}
