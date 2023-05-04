using System;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Clashes;

using RevitOpeningPlacement.Models.Exceptions;
using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement {
    internal class OpeningPlacer {
        private readonly RevitRepository _revitRepository;

        public OpeningPlacer(RevitRepository revitRepository, ClashModel clashModel = null) {
            if(revitRepository is null) {
                throw new ArgumentNullException(nameof(revitRepository));
            }

            _revitRepository = revitRepository;
            ClashModel = clashModel;
        }
        public ILevelFinder LevelFinder { get; set; }
        public IPointFinder PointFinder { get; set; }
        public IAngleFinder AngleFinder { get; set; }
        public IParametersGetter ParameterGetter { get; set; }
        public FamilySymbol Type { get; set; }
        /// <summary>
        /// Свойство, предоставляющее информацию об элементах, по пересечению которых должно размещаться текущее отверстие
        /// </summary>
        public ClashModel ClashModel { get; }

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
