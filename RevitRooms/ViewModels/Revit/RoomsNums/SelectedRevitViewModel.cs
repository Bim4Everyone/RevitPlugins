using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

using RevitRooms.Models;

namespace RevitRooms.ViewModels.Revit.RoomsNums {
    internal class SelectedRevitViewModel : RoomsNumsViewModel {
        public SelectedRevitViewModel(Application application, Document document)
            : base(application, document) {
            _id = new Guid("AAAC541D-16B3-4E82-A702-208B099AB031");
        }

        protected override IEnumerable<SpatialElementViewModel> GetSpartialElements() {
            return _revitRepository.GetSelectedSpatialElements()
                .Select(item => new SpatialElementViewModel(item, _revitRepository));
        }
    }
}
