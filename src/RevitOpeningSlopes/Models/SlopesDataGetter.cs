using System;

using Autodesk.Revit.DB;

namespace RevitOpeningSlopes.Models {
    internal class SlopesDataGetter {
        private readonly RevitRepository _revitRepository;
        private readonly LinesFromOpening _linesFromOpening;
        private readonly NearestElements _nearestElements;
        private readonly SolidOperations _solidOperations;

        public SlopesDataGetter(RevitRepository revitRepository, LinesFromOpening linesFromOpening,
            NearestElements nearestElements, SolidOperations solidOperations) {
            _revitRepository = revitRepository
                ?? throw new ArgumentNullException(nameof(revitRepository));
            _linesFromOpening = linesFromOpening
                ?? throw new ArgumentNullException(nameof(linesFromOpening));
            _nearestElements = nearestElements
                ?? throw new ArgumentNullException(nameof(nearestElements));
            _solidOperations = solidOperations
                ?? throw new ArgumentNullException(nameof(solidOperations));
        }

        /// <summary>
        /// Возвращает информацию для размещения экземпляра откоса
        /// </summary>
        /// <param name="config">Настройки плагина</param>
        /// <param name="opening">Окно</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Срабатывает, если глубина проема равна или меньше нуля</exception>
        public SlopeCreationData GetOpeningSlopeCreationData(PluginConfig config, FamilyInstance opening) {

            OpeningHandler openingParameters = new OpeningHandler(_revitRepository, _linesFromOpening,
                _nearestElements, _solidOperations, opening);
            double height = openingParameters.GetOpeningHeight();
            double width = openingParameters.GetOpeningWidth();
            double depth = openingParameters.GetOpeningDepth()
                + _revitRepository.ConvertToFeet(config.SlopeFrontOffset.Value);

            if(_revitRepository.ConvertToMillimeters(depth) < 1) {
                throw new ArgumentException("Глубина проема не может быть равна или меньше нуля");
            }
            XYZ center = openingParameters.GetVerticalCenterPoint();
            double rotationAngle = openingParameters.GetRotationAngle();

            SlopeCreationData slopeData = new SlopeCreationData() {
                Height = height,
                Width = width,
                Depth = depth,
                Center = center,
                SlopeTypeId = config.SlopeTypeId,
                RotationRadiansAngle = rotationAngle
            };
            return slopeData;
        }
    }
}

