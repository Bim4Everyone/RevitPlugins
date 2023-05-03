using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement {
    internal class OpeningPlacer {
        private readonly RevitRepository _revitRepository;

        public OpeningPlacer(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
        }
        public ILevelFinder LevelFinder { get; set; }
        public IPointFinder PointFinder { get; set; }
        public IAngleFinder AngleFinder { get; set; }
        public IParametersGetter ParameterGetter { get; set; }
        public FamilySymbol Type { get; set; }

        public FamilyInstance Place() {
            var point = PointFinder.GetPoint().Round();
            var level = LevelFinder.GetLevel();
            var opening = _revitRepository.CreateInstance(Type, point, level);

            var angles = AngleFinder.GetAngle();
            _revitRepository.RotateElement(opening, point, angles);

            SetParamValues(opening);

            return opening;
        }

        private void SetParamValues(FamilyInstance opening) {
            foreach(var paramValue in ParameterGetter.GetParamValues()) {
                paramValue.Value.SetParamValue(opening, paramValue.ParamName);
            }
        }
    }
}
