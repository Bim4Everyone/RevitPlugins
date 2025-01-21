using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

            ErrorRooms = new ObservableCollection<RoomErrorViewModel>(
                GetErrorRooms().Select(roomError => new RoomErrorViewModel(_revitRepository, 
                                                                           roomError, 
                                                                           ShowElementCommand)));
            ShowElementCommand = RelayCommand.Create<ElementId>(ShowElement);
        }
        
        public ICommand ShowElementCommand { get; }        
        public ObservableCollection<RoomErrorViewModel> ErrorRooms { get; set; }

        private void ShowElement(ElementId elementId) {
            _revitRepository.SetSelectedRoom(elementId);
        }

        private List<Room> GetErrorRooms() {            
            RoomChecker roomChecker = new RoomChecker(_revitRepository);
            return new List<Room>(
                _revitRepository.GetSelectedRooms()
                .Where(room => roomChecker.CheckInvalidRoom(room)));                
             
        }
    }    
}
