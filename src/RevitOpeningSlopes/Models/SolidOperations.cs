using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit.Geometry;

using RevitOpeningSlopes.Models.Exceptions;

namespace RevitOpeningSlopes.Models {
    internal class SolidOperations {
        private readonly RevitRepository _revitRepository;

        public SolidOperations(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
        }

        public Solid GetUnitedSolid(IEnumerable<Solid> solids) {
            return SolidExtensions.CreateUnitedSolids((IList<Solid>) solids)
                .OrderByDescending(s => s.Volume)
                .FirstOrDefault();
        }

        public Solid GetUnitedSolidFromOpening(Element opening) {
            if(opening == null)
                throw new ArgumentNullException(nameof(opening));
            ElementCategoryFilter categoryFilter = new ElementCategoryFilter(BuiltInCategory.OST_Windows);
            IList<ElementId> dependingElements = opening.GetDependentElements(categoryFilter);
            IList<Solid> totalSolids = new List<Solid>();
            foreach(ElementId depEl in dependingElements) {
                Solid openingSolid = GetUnitedSolid(_revitRepository.Document.GetElement(depEl).GetSolids());
                if(openingSolid.Volume > 0) {
                    totalSolids.Add(openingSolid);
                }
            }
            if(totalSolids.Count > 0) {
                return GetUnitedSolid(totalSolids);
            } else {
                throw new OpeningNullSolidException();
            }
        }

        public void CreateDirectShape(Solid solid) {
            DirectShape ds = DirectShape.CreateElement(_revitRepository.Document,
                new ElementId(BuiltInCategory.OST_GenericModel));
            ds.SetShape(new GeometryObject[] { solid });
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
