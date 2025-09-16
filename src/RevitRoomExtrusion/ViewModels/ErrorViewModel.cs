using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;

using RevitRoomExtrusion.Models;


namespace RevitRoomExtrusion.ViewModels;
internal class ErrorViewModel {
    private readonly ILocalizationService _localizationService;
    private readonly RevitRepository _revitRepository;
    private readonly RoomChecker _roomChecker;

    public ErrorViewModel(
        ILocalizationService localizationService, RevitRepository revitRepository, RoomChecker roomChecker) {
        _localizationService = localizationService;
        _revitRepository = revitRepository;
        _roomChecker = roomChecker;

        ShowElementCommand = RelayCommand.Create<ElementId>(ShowElement);
    }

    public ICommand ShowElementCommand { get; }
    public ObservableCollection<RoomErrorViewModel> ErrorRooms => GetErrorRooms();

    // Метод выделения помещения
    private void ShowElement(ElementId elementId) {
        _revitRepository.SetSelectedRoom(elementId);
    }

    // Метод получения неправильных помещений
    private ObservableCollection<RoomErrorViewModel> GetErrorRooms() {
        return new ObservableCollection<RoomErrorViewModel>(
            _revitRepository.GetSelectedRooms()
            .Where(_roomChecker.CheckInvalidRoom)
            .Select(roomError => new RoomErrorViewModel(
                _localizationService, _roomChecker, roomError, ShowElementCommand)));
    }
}
