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

        public Solid GetUnitedSolidFromHostWall(Wall wall) {
            IList<Solid> solids = GetJoinedWalls(wall)
                .Select(w => w.GetSolids())
                .SelectMany(ws => ws)
                .ToList();
            IList<Solid> unitedSolids = SolidExtensions.CreateUnitedSolids(solids);
            return unitedSolids.OrderByDescending(s => s.Volume)
                .FirstOrDefault();
        }

        private ICollection<Wall> GetJoinedWalls(Wall wall) {
            ICollection<ElementId> joinedElementsId = JoinGeometryUtils
                .GetJoinedElements(_revitRepository.Document, wall);
            return new FilteredElementCollector(_revitRepository.Document, joinedElementsId)
                .OfClass(typeof(Wall))
                .Cast<Wall>()
                .ToList();
        }
    }
}
