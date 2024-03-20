using System;

using Autodesk.Revit.DB;

using RevitOpeningSlopes.Models;

namespace RevitOpeningSlopes.Services.ValueGetter {
    internal class OpeningHeightGetter {
        private readonly RevitRepository _revitRepository;
        private readonly OpeningTopXYZGetter _openingTopXYZGetter;

        public OpeningHeightGetter(RevitRepository revitRepository, OpeningTopXYZGetter openingTopXYZGetter) {
            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
            _openingTopXYZGetter = openingTopXYZGetter ?? throw new ArgumentNullException(nameof(openingTopXYZGetter));
        }
        public double GetOpeningHeight(FamilyInstance opening) {
            XYZ origin = _revitRepository.GetOpeningLocation(opening);
            XYZ topPoint = _openingTopXYZGetter.GetOpeningTopXYZ(opening);
            if(topPoint == null) {
                return 0;
            } else {
                return origin.DistanceTo(topPoint);
            }
        }
    }
}
