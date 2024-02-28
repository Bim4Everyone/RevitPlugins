using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitOpeningSlopes.Models {
    internal class OpeningParams {
        private readonly RevitRepository _revitRepository;
        private readonly LinesFromOpening _linesFromOpening;
        private readonly SolidOperations _solidOperations;
        public OpeningParams(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
            _linesFromOpening = new LinesFromOpening(revitRepository);
            _solidOperations = new SolidOperations(revitRepository);
        }
        public XYZ GetOpeningCenter(FamilyInstance opening) {
            BoundingBoxXYZ bbox = opening.GetBoundingBox();
            XYZ origin = _revitRepository.GetOpeningLocation(opening);
            double openingCenterZ = (origin.Z + bbox.Max.Z) / 2;
            XYZ openingCenter = new XYZ(origin.X, origin.Y, openingCenterZ);
            return openingCenter;
        }
        //private Element GetElementByRay(View3D view3D, Curve curve, ElementFilter elementFilter) {
        //    XYZ lineDirection = (curve.GetEndPoint(1) - curve.GetEndPoint(0)).Normalize();
        //    ReferenceIntersector intersector
        //        = new ReferenceIntersector(elementFilter, FindReferenceTarget.Element, view3D) {
        //            FindReferencesInRevitLinks = false
        //        };
        //}
        //public double GetOpeningHeight(FamilyInstance opening) {
        //    //BoundingBoxXYZ bbox = opening.GetBoundingBox();
        //    //XYZ origin = _revitRepository.GetOpeningLocation(opening);
        //    //XYZ topPoint = new XYZ(origin.X, origin.Y, bbox.Max.Z);
        //    //double g = _revitRepository.ConvertToMillimeters(origin.DistanceTo(topPoint));
        //    //return origin.DistanceTo(topPoint);

        //}

    }
}
