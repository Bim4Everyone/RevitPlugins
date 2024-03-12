using Autodesk.Revit.DB;

namespace RevitOpeningSlopes.Models {
    internal class NearestElements {
        private readonly RevitRepository _revitRepository;


        public NearestElements(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
        }
        public Element GetElementByRay(Curve curve) {
            XYZ lineDirection = (curve.GetEndPoint(1) - curve.GetEndPoint(0)).Normalize();
            ElementMulticategoryFilter categoryFilter = new ElementMulticategoryFilter(
                new BuiltInCategory[] {
                    BuiltInCategory.OST_Walls,
                    BuiltInCategory.OST_Floors});
            Element currentElement = null;
            ReferenceIntersector intersector
                = new ReferenceIntersector(categoryFilter, FindReferenceTarget.Element,
                _revitRepository.Default3DView) {
                    FindReferencesInRevitLinks = false
                };
            //ReferenceIntersector intersector
            //    = new ReferenceIntersector(_revitRepository.Default3DView) {
            //        FindReferencesInRevitLinks = false
            //    };
            ReferenceWithContext context = intersector.FindNearest(curve.GetEndPoint(0), lineDirection);

            Reference closestReference;
            if(context != null) {
                closestReference = context.GetReference();
                if(closestReference != null) {
                    currentElement = _revitRepository.Document.GetElement(closestReference);
                }
            }
            return currentElement;
        }
    }
}
