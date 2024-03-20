using System;

using Autodesk.Revit.DB;

using RevitOpeningSlopes.Models;

namespace RevitOpeningSlopes.Services.ValueGetter {
    internal class OpeningCenterXYZGetter {
        private readonly RevitRepository _revitRepository;
        private readonly OpeningTopXYZGetter _openingTopXYZGetter;

        public OpeningCenterXYZGetter(
            RevitRepository revitRepository,
            OpeningTopXYZGetter openingTopXYZGetter) {
            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
            _openingTopXYZGetter = openingTopXYZGetter ?? throw new ArgumentNullException(nameof(openingTopXYZGetter));
        }
        public XYZ GetOpeningCenter(FamilyInstance opening) {
            XYZ origin = _revitRepository.GetOpeningLocation(opening);
            double halfHeight = origin.DistanceTo(_openingTopXYZGetter.GetOpeningTopXYZ(opening)) / 2;
            return new XYZ(origin.X, origin.Y, origin.Z + halfHeight);
        }
    }
}
