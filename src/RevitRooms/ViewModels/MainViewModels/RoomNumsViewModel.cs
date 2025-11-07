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
using RevitRooms.ViewModels.Revit.RoomsNums;
using RevitRooms.Views;

namespace RevitRooms.ViewModels;
internal class RoomNumsViewModel : BaseViewModel {
    private readonly RoomsNumsConfig _roomsConfig;
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;
    private readonly RoomsNumsWindow _window;

    private ObservableCollection<RoomsNumsViewModel> _roomsNumsViewModels;
    private RoomsNumsViewModel _roomsNums;

    public RoomNumsViewModel(RoomsNumsConfig roomsConfig,
                             RevitRepository revitRepository,
                             ILocalizationService localizationService,
                             RoomsNumsWindow window) {
        _roomsConfig = roomsConfig;
        _revitRepository = revitRepository;
        _localizationService = localizationService;
        _window = window;

        LoadViewCommand = RelayCommand.Create(LoadView);
    }

    public ICommand LoadViewCommand { get; }

    public RoomsNumsViewModel RoomsNums {
        get => _roomsNums;
        set => RaiseAndSetIfChanged(ref _roomsNums, value);
    }

    public ObservableCollection<RoomsNumsViewModel> RoomsNumsViewModels {
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
            new ViewRevitViewModel(_revitRepository) { 
                Name = "Выборка по текущему виду",
                RoomsNumsConfig = _roomsConfig,
                ParentWindow = _window
            },
            new ElementsRevitViewModel(_revitRepository) { 
                Name = "Выборка по всем элементам",
                RoomsNumsConfig = _roomsConfig,
                ParentWindow = _window
            },
            new SelectedRevitViewModel(_revitRepository) { 
                Name = "Выборка по выделенным элементам",
                RoomsNumsConfig = _roomsConfig,
                ParentWindow = _window
            }
        ];

        RoomsNums = RoomsNumsViewModels[1];

        var settings = _roomsConfig.GetSettings(_revitRepository.DocumentName);
        if(settings != null) {
            RoomsNums = RoomsNumsViewModels
                .FirstOrDefault(item => item._id == settings.SelectedRoomId) ?? RoomsNums;
        }
    }
}
