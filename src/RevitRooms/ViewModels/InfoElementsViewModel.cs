using System.Collections.ObjectModel;

using dosymep.WPF.ViewModels;

namespace RevitRooms.ViewModels;
internal class InfoElementsViewModel : BaseViewModel {
    private InfoElementViewModel _infoElement;

    public InfoElementViewModel InfoElement {
        get => _infoElement;
        set => RaiseAndSetIfChanged(ref _infoElement, value);
    }

    public ObservableCollection<InfoElementViewModel> InfoElements { get; set; }
}
