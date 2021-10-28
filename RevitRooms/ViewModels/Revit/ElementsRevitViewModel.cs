using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

namespace RevitRooms.ViewModels.Revit {
    internal class ElementsRevitViewModel : RevitViewModel {
        public ElementsRevitViewModel(Application application, Document document)
            : base(application, document) {
        }

        protected override IEnumerable<RoomViewModel> GetRoomViewModels() {
            return _revitRepository.GetAllRooms()
                .Select(item => new RoomViewModel(item, _revitRepository));
        }
    }
}
