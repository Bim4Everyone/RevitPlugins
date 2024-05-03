using System;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Clashes;

using RevitOpeningPlacement.Models.Exceptions;
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


        /// <summary>
        /// Размещает семейство по заданным настройкам
        /// </summary>
        /// <returns></returns>
        /// <exception cref="OpeningNotPlacedException">Исключение, если не удалось разместить семейство с заданными настройками</exception>
        public FamilyInstance Place() {
            XYZ point;
            try {
                point = PointFinder.GetPoint();
            } catch(Exception ex) when(
            ex is IntersectionNotFoundException
            || ex is NullReferenceException
            || ex is ArgumentNullException
            || ex is Autodesk.Revit.Exceptions.ArgumentNullException) {

                throw new OpeningNotPlacedException("Не удалось найти точку вставки");
            }

            var level = LevelFinder.GetLevel();
            var opening = _revitRepository.CreateInstance(Type, point, level);

            var angles = AngleFinder.GetAngle();
            _revitRepository.RotateElement(opening, point, angles);

            try {
                SetParamValues(opening);
            } catch(System.NullReferenceException) {
                _revitRepository.DeleteElement(opening.Id);
                throw new OpeningNotPlacedException("Не удалось назначить параметры созданного отверстия, вследствие чего оно было удалено");
            } catch(SizeTooSmallException e) {
                _revitRepository.DeleteElement(opening.Id);
                throw new OpeningNotPlacedException($"{e.Message}, отверстие было удалено");
            }
            return opening;
        }

        private void SetParamValues(FamilyInstance opening) {
            foreach(var paramValue in ParameterGetter.GetParamValues()) {
                paramValue.Value.SetParamValue(opening, paramValue.ParamName);
            }
        }
    }
}
