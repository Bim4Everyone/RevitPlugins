using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

using RevitRooms.Models;

namespace RevitRooms.ViewModels.Revit.RoomsNums {
    internal class ViewRevitViewModel : RoomsNumsViewModel {
        public ViewRevitViewModel(Application application, Document document)
            : base(application, document) {
            _id = new Guid("38DF60C2-1D99-4256-9D41-0CB34A95E0AE");
        }

        protected override IEnumerable<SpatialElementViewModel> GetSpartialElements() {
            return _revitRepository.GetRoomsOnActiveView()
                .Select(item => new SpatialElementViewModel(item, _revitRepository));
        }
    }
}