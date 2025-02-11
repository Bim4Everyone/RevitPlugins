using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit.Geometry;

namespace RevitRoughFinishingDesign.Models {
    internal class SolidOperations {
        private readonly RevitRepository _revitRepository;

        public SolidOperations(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
        }

        public Solid GetSolidFromWall(Wall wall) {
            Options options = new Options() { DetailLevel = ViewDetailLevel.Fine };
            Solid wallSolid = wall.GetSolids(options).First();
            return wallSolid;
        }
    }
}
