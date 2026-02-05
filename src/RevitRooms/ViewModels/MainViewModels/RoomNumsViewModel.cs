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
using RevitRooms.ViewModels.RoomsNums;
using RevitRooms.Views;

namespace RevitRooms.ViewModels;
internal class RoomNumsViewModel : BaseViewModel {
    private readonly RoomsNumsConfig _roomsNumsConfig;
    private readonly NumOrderWindowService _numberingWindowService;
    private readonly RevitRepository _revitRepository;
    private readonly IMessageBoxService _messageBoxService;
    private readonly ErrorWindowService _errorWindowService;
    private readonly ILocalizationService _localizationService;
    private readonly RoomsNumsWindow _window;

    private ObservableCollection<RevitRoomNumsViewModel> _roomsNumsViewModels;
    private RevitRoomNumsViewModel _roomsNums;

    public RoomNumsViewModel(RoomsNumsConfig roomsNumsConfig,
                             RevitRepository revitRepository,
                             ILocalizationService localizationService,
                             IMessageBoxService messageBoxService,
                             RoomsNumsWindow window,
                             NumOrderWindowService numberingWindowService,
                             ErrorWindowService errorWindowService) {
        _numberingWindowService = numberingWindowService;
        _roomsNumsConfig = roomsNumsConfig;
        _revitRepository = revitRepository;
        _localizationService = localizationService;
        _messageBoxService = messageBoxService;
        _errorWindowService = errorWindowService;
        _window = window;

        LoadViewCommand = RelayCommand.Create(LoadView);
    }

    public ICommand LoadViewCommand { get; }

    public RevitRoomNumsViewModel RoomsNums {
        get => _roomsNums;
        set => RaiseAndSetIfChanged(ref _roomsNums, value);
    }

    public ObservableCollection<RevitRoomNumsViewModel> RoomsNumsViewModels {
        get => _roomsNumsViewModels;
        set => RaiseAndSetIfChanged(ref _roomsNumsViewModels, value);
    }

    public string NumberParamName => LabelUtils.GetLabelFor(BuiltInParameter.ROOM_NUMBER);
    public string ApartmentNumberParamName => SharedParamsConfig.Instance.ApartmentNumber.Name;

    private void LoadView() {
        RoomsNumsViewModels = [
            new ViewRevitViewModel(_revitRepository, _roomsNumsConfig, _messageBoxService, 
                                   _localizationService, _numberingWindowService, _errorWindowService) { 
                Name = _localizationService.GetLocalizedString("MainWindow.SelectView"),
                ParentWindow = _window
            },
            new ElementsRevitViewModel(_revitRepository, _roomsNumsConfig, _messageBoxService, 
                                       _localizationService, _numberingWindowService, _errorWindowService) {
                Name = _localizationService.GetLocalizedString("MainWindow.SelectAll"),
                ParentWindow = _window
            },
            new SelectedRevitViewModel(_revitRepository, _roomsNumsConfig, _messageBoxService, 
                                       _localizationService, _numberingWindowService, _errorWindowService) { 
                Name = _localizationService.GetLocalizedString("MainWindow.SelectSelected"),
                ParentWindow = _window
            }
        ];

        RoomsNums = RoomsNumsViewModels[1];

        var settings = _roomsNumsConfig.GetSettings(_revitRepository.DocumentName);
        if(settings != null) {
            RoomsNums = RoomsNumsViewModels
                .FirstOrDefault(item => item._id == settings.SelectedRoomId) ?? RoomsNums;
        }
    }
}
