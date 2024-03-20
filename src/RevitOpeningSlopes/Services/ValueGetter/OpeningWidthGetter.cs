using System;

using Autodesk.Revit.DB;

namespace RevitOpeningSlopes.Services.ValueGetter {
    internal class OpeningWidthGetter {
        private readonly OpeningCenterXYZGetter _openingCenterXYZGetter;
        private readonly OpeningRightXYZGetter _openingRightXYZGetter;

        public OpeningWidthGetter(
            OpeningCenterXYZGetter openingCenterXYZGetter,
            OpeningRightXYZGetter openingRightXYZGetter) {

            _openingCenterXYZGetter = openingCenterXYZGetter
                ?? throw new ArgumentNullException(nameof(openingCenterXYZGetter));
            _openingRightXYZGetter = openingRightXYZGetter
                ?? throw new ArgumentNullException(nameof(openingRightXYZGetter));
        }
        public double GetOpeningWidth(FamilyInstance opening) {
            XYZ center = _openingCenterXYZGetter.GetOpeningCenter(opening);
            XYZ rightPoint = _openingRightXYZGetter.GetOpeningRightXYZ(opening);
            if(center == null || rightPoint == null) {
                return 0;
            } else {
                return center.DistanceTo(rightPoint) * 2;
            }
        }
    }
}
