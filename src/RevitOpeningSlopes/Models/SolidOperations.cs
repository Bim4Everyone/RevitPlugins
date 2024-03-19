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
                Solid hostSolid = element.GetSolids().FirstOrDefault();
                ICollection<ElementId> elementIds = JoinGeometryUtils
                    .GetJoinedElements(_revitRepository.Document, element);

                IList<Solid> solids = elementIds
                    .Select(el => _revitRepository.Document.GetElement(el).GetSolids())
                    .SelectMany(els => els)
                    .ToList();

                solids.Add(hostSolid);

                IList<Solid> unitedSolids = SolidExtensions.CreateUnitedSolids(solids);
                return unitedSolids.OrderByDescending(s => s.Volume)
                    .FirstOrDefault();
            }
        }
    }
}
