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
    private readonly ILocalizationService _localizationService;
    private readonly RoomsNumsWindow _window;

    private ObservableCollection<RevitViewModel> _roomsNumsViewModels;
    private RevitViewModel _roomsNums;

    public RoomNumsViewModel(RoomsNumsConfig roomsNumsConfig,
                             RevitRepository revitRepository,
                             ILocalizationService localizationService,
                             RoomsNumsWindow window,
                             NumOrderWindowService numberingWindowService) {
        _numberingWindowService = numberingWindowService;
        _roomsNumsConfig = roomsNumsConfig;
        _revitRepository = revitRepository;
        _localizationService = localizationService;
        _window = window;

        LoadViewCommand = RelayCommand.Create(LoadView);
    }

    public ICommand LoadViewCommand { get; }

    public RevitViewModel RoomsNums {
        get => _roomsNums;
        set => RaiseAndSetIfChanged(ref _roomsNums, value);
    }

    public ObservableCollection<RevitViewModel> RoomsNumsViewModels {
        get => _roomsNumsViewModels;
        set => RaiseAndSetIfChanged(ref _roomsNumsViewModels, value);
    }

    public string NumberParamName => LabelUtils.GetLabelFor(BuiltInParameter.ROOM_NUMBER);
    public string ApartmentNumberParamName => SharedParamsConfig.Instance.ApartmentNumber.Name;

    private void LoadView() {
        bool isChecked = new CheckProjectParams(_revitRepository.UIApplication)
            .CopyProjectParams()
            .CopyKeySchedules()
            .CheckKeySchedules()
            .GetIsChecked();

        if(!isChecked) {
            throw new OperationCanceledException();
        }

        RoomsNumsViewModels = [
            new ViewRevitViewModel(_revitRepository, _roomsNumsConfig, _numberingWindowService) { 
                Name = "Выборка по текущему виду",
                ParentWindow = _window
            },
            new ElementsRevitViewModel(_revitRepository, _roomsNumsConfig, _numberingWindowService) { 
                Name = "Выборка по всем элементам",
                ParentWindow = _window
            },
            new SelectedRevitViewModel(_revitRepository, _roomsNumsConfig, _numberingWindowService) { 
                Name = "Выборка по выделенным элементам",
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
