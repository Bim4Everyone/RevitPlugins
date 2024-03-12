using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit.Geometry;

namespace RevitOpeningSlopes.Models {
    internal class SolidOperations {
        private readonly RevitRepository _revitRepository;

        public SolidOperations(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
        }

        public Solid GetUnitedSolidFromHostElement(Element element) {
            if(element == null) {
                throw new ArgumentNullException(nameof(element));
            } else {

                ICollection<ElementId> elementIds = JoinGeometryUtils
                    .GetJoinedElements(_revitRepository.Document, element);

                IList<Solid> solids = elementIds
                    .Select(el => _revitRepository.Document.GetElement(el).GetSolids())
                    .SelectMany(els => els)
                    .ToList();

                IList<Solid> unitedSolids = SolidExtensions.CreateUnitedSolids(solids);
                return unitedSolids.OrderByDescending(s => s.Volume)
                    .FirstOrDefault();
            }
        }

        //private ICollection<Element> GetJoinedElements(Element element) {
        //    ICollection<ElementId> joinedElementsId = JoinGeometryUtils
        //        .GetJoinedElements(_revitRepository.Document, element);
        //    return new FilteredElementCollector(_revitRepository.Document, joinedElementsId)
        //        .OfClass(typeof(Element))
        //        .Cast<Element>()
        //        .ToList();
        //}
    }
}
