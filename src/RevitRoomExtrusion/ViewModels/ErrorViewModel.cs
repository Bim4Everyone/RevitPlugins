using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

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

        ErrorRooms = new ObservableCollection<RoomErrorViewModel>(
            GetErrorRooms().Select(roomError => new RoomErrorViewModel(
                _localizationService, _roomChecker, roomError, ShowElementCommand)));

        ShowElementCommand = RelayCommand.Create<ElementId>(ShowElement);
    }

    public ICommand ShowElementCommand { get; }
    public ObservableCollection<RoomErrorViewModel> ErrorRooms { get; set; }

    private void ShowElement(ElementId elementId) {
        _revitRepository.SetSelectedRoom(elementId);
    }

    private List<Room> GetErrorRooms() {
        return _revitRepository.GetSelectedRooms()
            .Where(_roomChecker.CheckInvalidRoom)
            .ToList();
    }
}
