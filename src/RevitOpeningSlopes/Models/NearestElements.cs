using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitOpeningSlopes.Models {
    internal class NearestElements {
        private readonly RevitRepository _revitRepository;
        private readonly ElementFilter _categoryFilter = new ElementMulticategoryFilter(
                new BuiltInCategory[] {
                    BuiltInCategory.OST_Walls,
                    BuiltInCategory.OST_Columns,
                    BuiltInCategory.OST_StructuralColumns,
                    BuiltInCategory.OST_StructuralFraming,
                    BuiltInCategory.OST_Floors,
                    BuiltInCategory.OST_GenericModel,
                    BuiltInCategory.OST_Doors});
        public NearestElements(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
        }

        /// <summary>
        /// Функция возвращает список элементов, которых пересек луч в направлении указанной кривой
        /// </summary>
        /// <param name="curve">Кривая для направления луча</param>
        /// <returns>Элементы, которые пересек луч</returns>
        public IList<Element> GetElementsByRay(Curve curve) {
            XYZ lineDirection = (curve.GetEndPoint(1) - curve.GetEndPoint(0)).Normalize();

            IList<Element> elements = new List<Element>();
            ReferenceIntersector intersector
                = new ReferenceIntersector(_categoryFilter, FindReferenceTarget.All,
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
