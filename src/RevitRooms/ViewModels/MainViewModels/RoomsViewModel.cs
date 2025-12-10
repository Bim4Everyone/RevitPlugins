using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone.SharedParams;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitRooms.Models;
using RevitRooms.Services;
using RevitRooms.ViewModels.Rooms;

namespace RevitRooms.ViewModels;
internal class RoomsViewModel : BaseViewModel {
    private readonly RoomsConfig _roomsConfig;
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;
    private readonly IMessageBoxService _messageBoxService;
    private readonly ErrorWindowService _errorWindowService;

    private ObservableCollection<RevitRoomsViewModel> _revitViewModels;
    private RevitRoomsViewModel _revitViewModel;

    public RoomsViewModel(RoomsConfig roomsConfig,
                          RevitRepository revitRepository,
                          ILocalizationService localizationService,
                          IMessageBoxService messageBoxService,
                          ErrorWindowService errorWindowService) {
        _roomsConfig = roomsConfig;
        _revitRepository = revitRepository;
        _localizationService = localizationService;
        _messageBoxService = messageBoxService;
        _errorWindowService = errorWindowService;

        LoadViewCommand = RelayCommand.Create(LoadView);
    }

    public ICommand LoadViewCommand { get; }

    public RevitRoomsViewModel RevitViewModel {
        get => _revitViewModel;
        set => RaiseAndSetIfChanged(ref _revitViewModel, value);
    }

    public ObservableCollection<RevitRoomsViewModel> RevitViewModels {
        get => _revitViewModels;
        set => RaiseAndSetIfChanged(ref _revitViewModels, value);
    }

    public string LevelParamName => SharedParamsConfig.Instance.Level.Name;

    private void LoadView() {
        RevitViewModels = [
            new ViewRevitViewModel(_revitRepository, _roomsConfig, _messageBoxService, 
                                   _localizationService, _errorWindowService) {
                Name = _localizationService.GetLocalizedString("MainWindow.SelectView")
            },
            new ElementsRevitViewModel(_revitRepository, _roomsConfig, _messageBoxService,
                                       _localizationService, _errorWindowService) {
                Name = _localizationService.GetLocalizedString("MainWindow.SelectAll") 
            },
            new SelectedRevitViewModel(_revitRepository, _roomsConfig, _messageBoxService, 
                                       _localizationService, _errorWindowService) {
                Name = _localizationService.GetLocalizedString("MainWindow.SelectSelected")
            },
        ];

        RevitViewModel = RevitViewModels[1];

        var settings = _roomsConfig.GetSettings(_revitRepository.Document);
        if(settings != null) {
            RevitViewModel = RevitViewModels
                .FirstOrDefault(item => item._id == settings.SelectedRoomId) ?? RevitViewModel;
        }
    }
}
