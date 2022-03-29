using System.Collections.Generic;
using System.Linq;
using System;

using Autodesk.Revit.DB;

namespace RevitMarkPlacement.Models {
    internal class AllElementsSelection : ISelectionMode {
        public IEnumerable<SpotDimension> GetSpotDimentions(Document doc) {
            return new FilteredElementCollector(doc)
            .OfClass(typeof(SpotDimension))
            .OfType<SpotDimension>()
            .Where(IsCorrectSpot);
        }

        private bool IsCorrectSpot(SpotDimension spot) {
            return GetHasLeader(spot) &&
                (string.IsNullOrEmpty(RevitRepository.FilterSpotName)
             || spot.Name.EndsWith(RevitRepository.FilterSpotName, StringComparison.CurrentCultureIgnoreCase));
        }

#if D2020 || R2020
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
