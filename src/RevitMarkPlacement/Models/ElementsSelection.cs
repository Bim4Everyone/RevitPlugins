using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Revit;

namespace RevitMarkPlacement.Models {

    internal class ElementsSelection : ISelectionMode {
        public IEnumerable<SpotDimension> GetSpotDimentions(Document doc) {
            return new UIDocument(doc).GetSelectedElements()
                .OfType<SpotDimension>()
                .Where(IsCorrectSpot);
        }

        private bool IsCorrectSpot(SpotDimension spot) {
            return GetHasLeader(spot) &&
                (string.IsNullOrEmpty(RevitRepository.FilterSpotName)
             || spot.Name.EndsWith(RevitRepository.FilterSpotName, StringComparison.CurrentCultureIgnoreCase));
        }

#if REVIT_2021_OR_LESS
        private static bool GetHasLeader(SpotDimension spot) {
            return true;
        }
#else
        private static bool GetHasLeader(SpotDimension spot) {
            return spot.HasLeader;
        }
#endif
    }
}
