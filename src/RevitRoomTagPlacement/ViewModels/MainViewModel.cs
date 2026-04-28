using System.Collections.ObjectModel;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitRoomTagPlacement.Models;

namespace RevitRoomTagPlacement.ViewModels;
internal class MainViewModel : BaseViewModel {
    private readonly RevitRepository _revitRepository;

    private RevitViewModel _revitViewModel;

    public MainViewModel(RevitRepository revitRepository, 
                         ILocalizationService localizationService) {
        _revitRepository = revitRepository;

        RevitViewModels = [
            new ViewRevitViewModel(revitRepository, localizationService) { 
                Name = localizationService.GetLocalizedString("MainWindow.ByCurrentView")
            },
            new SelectedRevitViewModel(revitRepository, localizationService) { 
                Name = localizationService.GetLocalizedString("MainWindow.BySelectedElements")
            }
        ];

        RevitViewModel = _revitRepository.GetSelectedRooms().Count > 0 ? RevitViewModels[1] : RevitViewModels[0];

    }

    public RevitViewModel RevitViewModel {
        get => _revitViewModel;
        set => RaiseAndSetIfChanged(ref _revitViewModel, value);
    }

    public ObservableCollection<RevitViewModel> RevitViewModels { get; }
}
