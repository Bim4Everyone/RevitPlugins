using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitOpeningSlopes.Models {
    internal class NearestElements {
        private readonly RevitRepository _revitRepository;


        public NearestElements(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
        }
        //public Element GetElementByRay(Curve curve, bool onlyRoom = false) {
        //    XYZ lineDirection = (curve.GetEndPoint(1) - curve.GetEndPoint(0)).Normalize();
        //    ElementFilter categoryFilter = new ElementMulticategoryFilter(
        //        new BuiltInCategory[] {
        //            BuiltInCategory.OST_Walls,
        //            BuiltInCategory.OST_Columns,
        //            BuiltInCategory.OST_StructuralColumns,
        //            BuiltInCategory.OST_StructuralFraming,
        //            BuiltInCategory.OST_Floors});
        //    if(onlyRoom) {
        //        categoryFilter = new ElementCategoryFilter(BuiltInCategory.OST_Rooms);
        //    }
        //    Element currentElement = null;
        //    ReferenceIntersector intersector
        //        = new ReferenceIntersector(categoryFilter, FindReferenceTarget.All,
        //        _revitRepository.Default3DView) {
        //            FindReferencesInRevitLinks = false
        //        };

        //    ReferenceWithContext context = intersector.FindNearest(curve.GetEndPoint(0), lineDirection);

        //    Reference closestReference;
        //    if(context != null) {
        //        closestReference = context.GetReference();
        //        if(closestReference != null) {
        //            currentElement = _revitRepository.Document.GetElement(closestReference);
        //        }
        //    }
        //    return currentElement;
        //}

        public IList<Element> GetElementsByRay(Curve curve, bool onlyRoom = false) {
            XYZ lineDirection = (curve.GetEndPoint(1) - curve.GetEndPoint(0)).Normalize();
            ElementFilter categoryFilter = new ElementMulticategoryFilter(
                new BuiltInCategory[] {
                    BuiltInCategory.OST_Walls,
                    BuiltInCategory.OST_Columns,
                    BuiltInCategory.OST_StructuralColumns,
                    BuiltInCategory.OST_StructuralFraming,
                    BuiltInCategory.OST_Floors,
                    BuiltInCategory.OST_GenericModel,
                    BuiltInCategory.OST_Doors});
            if(onlyRoom) {
                categoryFilter = new ElementCategoryFilter(BuiltInCategory.OST_Rooms);
            }
            IList<Element> elements = new List<Element>();
            ReferenceIntersector intersector
                = new ReferenceIntersector(categoryFilter, FindReferenceTarget.All,
                _revitRepository.Default3DView) {
                    FindReferencesInRevitLinks = false
                };

            IList<ReferenceWithContext> contextList = intersector.Find(curve.GetEndPoint(0), lineDirection);

            if(contextList.Count > 0) {
                Reference elementReference;
                foreach(ReferenceWithContext context in contextList) {
                    elementReference = context.GetReference();
                    if(elementReference != null) {
                        elements.Add(_revitRepository.Document.GetElement(elementReference));
                    }

                }
            }
            return elements;
        }
    }
}
