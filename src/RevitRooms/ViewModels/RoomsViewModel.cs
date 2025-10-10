using System.Collections.ObjectModel;
using System.Linq;

using dosymep.Bim4Everyone.SharedParams;
using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitRooms.Models;
using RevitRooms.ViewModels.Revit;

namespace RevitRooms.ViewModels;
internal class RoomsViewModel : BaseViewModel {
    private RevitViewModel _revitViewModel;

    public RoomsViewModel(RoomsConfig roomsConfig,
                          RevitRepository revitRepository,
                          ILocalizationService localizationService) {


        RevitViewModels = [
            new ViewRevitViewModel(revitRepository, roomsConfig) {
                Name = localizationService.GetLocalizedString("MainWindow.ViewRevitName") 
            },
                //Name = "Выборка по текущему виду" },
            new ElementsRevitViewModel(revitRepository, roomsConfig) { 
                Name = "Выборка по всем элементам" },
            new SelectedRevitViewModel(revitRepository, roomsConfig) { 
                Name = "Выборка по выделенным элементам" }
        ];

        RevitViewModel = RevitViewModels[1];

        var settings = roomsConfig.GetSettings(revitRepository.Document);
        if(settings != null) {
            RevitViewModel = RevitViewModels.FirstOrDefault(item => item._id == settings.SelectedRoomId) ?? RevitViewModel;
        }
    }

    public RevitViewModel RevitViewModel {
        get => _revitViewModel;
        set => RaiseAndSetIfChanged(ref _revitViewModel, value);
    }

    public ObservableCollection<RevitViewModel> RevitViewModels { get; }
    public string LevelParamName => SharedParamsConfig.Instance.Level.Name;
}
