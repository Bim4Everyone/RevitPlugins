using Autodesk.Revit.DB;

namespace RevitSetLevelSection.Models {
    internal interface ILevelElevationService {
        double GetElevation(Level level);
    }

    internal class LevelElevationService : ILevelElevationService {
        private readonly BasePoint _basePoint;

        public LevelElevationService(BasePoint basePoint) {
            _basePoint = basePoint;
        }

        public double GetElevation(Level level) {
            return level.Elevation + _basePoint.Position.Z;
        }
    }
}