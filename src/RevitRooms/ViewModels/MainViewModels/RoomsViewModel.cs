using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using DevExpress.XtraExport.Helpers;

using dosymep.Bim4Everyone.SharedParams;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitRooms.Models;
using RevitRooms.ViewModels.Revit;

namespace RevitRooms.ViewModels;
internal class RoomsViewModel : BaseViewModel {
    private readonly RoomsConfig _roomsConfig;
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;

    private ObservableCollection<RevitViewModel> _revitViewModels;
    private RevitViewModel _revitViewModel;

    public RoomsViewModel(RoomsConfig roomsConfig,
                          RevitRepository revitRepository,
                          ILocalizationService localizationService) {
        _roomsConfig = roomsConfig;
        _revitRepository = revitRepository;
        _localizationService = localizationService;

        LoadViewCommand = RelayCommand.Create(LoadView);
    }

    public ICommand LoadViewCommand { get; }

    public RevitViewModel RevitViewModel {
        get => _revitViewModel;
        set => RaiseAndSetIfChanged(ref _revitViewModel, value);
    }

    public ObservableCollection<RevitViewModel> RevitViewModels {
        get => _revitViewModels;
        set => RaiseAndSetIfChanged(ref _revitViewModels, value);
    }

    public string LevelParamName => SharedParamsConfig.Instance.Level.Name;

    private void LoadView() {
        bool isChecked = new CheckProjectParams(_revitRepository.UIApplication)
            .CopyProjectParams()
            .CopyKeySchedules()
            .CheckKeySchedules()
            .GetIsChecked();

        if(!isChecked) {
            throw new OperationCanceledException();
        }

        RevitViewModels = [
            new ViewRevitViewModel(_revitRepository, _roomsConfig) {
                Name = "Выборка по текущему виду"
                //Name = _localizationService.GetLocalizedString("MainWindow.ViewRevitName")
            },
            new ElementsRevitViewModel(_revitRepository, _roomsConfig) {
                Name = "Выборка по всем элементам" },
            new SelectedRevitViewModel(_revitRepository, _roomsConfig) {
                Name = "Выборка по выделенным элементам" }
        ];

        RevitViewModel = RevitViewModels[1];

        var settings = _roomsConfig.GetSettings(_revitRepository.Document);
        if(settings != null) {
            RevitViewModel = RevitViewModels.FirstOrDefault(item => item._id == settings.SelectedRoomId) ?? RevitViewModel;
        }
    }
}
