using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

using RevitRooms.Models;

namespace RevitRooms.ViewModels.Revit.RoomsNums {
    internal class ElementsRevitViewModel : RoomsNumsViewModel {
        public ElementsRevitViewModel(RevitRepository revitRepository)
            : base(revitRepository) {
            _id = new Guid("19723C2C-75ED-4B0A-8279-8493A949E52F");
        }

        protected override IEnumerable<SpatialElementViewModel> GetSpatialElements() {
            return _revitRepository.GetSpatialElements()
                .Select(item => new SpatialElementViewModel(item, _revitRepository));
        }
    }
}
