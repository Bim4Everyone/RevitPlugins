using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using dosymep.WPF.Commands;

using RevitRoomExtrusion.Models;

namespace RevitRoomExtrusion.ViewModels {
    internal class ErrorViewModel {        
        private readonly RevitRepository _revitRepository;
        public ErrorViewModel(RevitRepository revitRepository) {
            
            _revitRepository = revitRepository;
            ErrorRooms = new ObservableCollection<RoomErrorElement>();
            
            foreach(Room roomError in GetErrorRooms()) { 
                ErrorRooms.Add(new RoomErrorElement(_revitRepository, roomError, ShowElementCommand));
            }            
            ShowElementCommand = RelayCommand.Create<ElementId>(ShowElement);
        }
        
        public ICommand ShowElementCommand { get; }        
        public ObservableCollection<RoomErrorElement> ErrorRooms { get; set; }

        private void ShowElement(ElementId elementId) {
            _revitRepository.SetSelectedRoom(elementId);
        }
        private List<Room> GetErrorRooms() {
            List<Room> result = new List<Room>();
            RoomChecker roomChecker = new RoomChecker(_revitRepository);
            foreach(Room room in _revitRepository.GetSelectedRooms()) {
                if(roomChecker.IsInValidRoom(room)) {
                    result.Add(room);
                }
            }
            return result;
        }
    }    
}
