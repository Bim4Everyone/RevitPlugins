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
        public ViewRevitViewModel(RevitRepository revitRepository) 
            : base(revitRepository) {
            _id = new Guid("38DF60C2-1D99-4256-9D41-0CB34A95E0AE");
        }

        protected override IEnumerable<SpatialElementViewModel> GetSpatialElements() {
            return _revitRepository.GetRoomsOnActiveView()
                .Select(item => new SpatialElementViewModel(item, _revitRepository));
        }
    }
}