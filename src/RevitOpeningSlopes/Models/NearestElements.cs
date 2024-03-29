using Autodesk.Revit.DB;

namespace RevitOpeningSlopes.Models {
    internal class NearestElements {
        private readonly RevitRepository _revitRepository;


        public NearestElements(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
        }
        public Element GetElementByRay(Curve curve, bool onlyWindow = false) {
            XYZ lineDirection = (curve.GetEndPoint(1) - curve.GetEndPoint(0)).Normalize();
            ElementFilter categoryFilter = new ElementMulticategoryFilter(
                new BuiltInCategory[] {
                    BuiltInCategory.OST_Walls,
                    BuiltInCategory.OST_Columns,
                    BuiltInCategory.OST_StructuralColumns,
                    BuiltInCategory.OST_StructuralFraming,
                    BuiltInCategory.OST_Floors});
            if(onlyWindow) {
                categoryFilter = new ElementCategoryFilter(BuiltInCategory.OST_Windows);
            }
            Element currentElement = null;
            ReferenceIntersector intersector
                = new ReferenceIntersector(categoryFilter, FindReferenceTarget.All,
                _revitRepository.Default3DView) {
                    FindReferencesInRevitLinks = false
                };

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
