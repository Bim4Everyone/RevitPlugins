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
            ErrorRooms = new ObservableCollection<ErrorRoomElement>();
            
            foreach(Room roomError in GetErrorRooms()) { 
                ErrorRooms.Add(new ErrorRoomElement(_revitRepository, roomError, ShowElementCommand));
            }            
            ShowElementCommand = RelayCommand.Create<ElementId>(ShowElement);
        }
        
        public ICommand ShowElementCommand { get; }        
        public ObservableCollection<ErrorRoomElement> ErrorRooms { get; set; }

        private void ShowElement(ElementId elementId) {
            _revitRepository.SetRoom(elementId);
        }
        private List<Room> GetErrorRooms() {
            List<Room> result = new List<Room>();
            RoomChecker roomChecker = new RoomChecker(_revitRepository);
            foreach(Room room in _revitRepository.GetRooms()) {
                if(roomChecker.IsInValidRoom(room)) {
                    result.Add(room);
                }
            }
            return result;
        }
    }    
}
