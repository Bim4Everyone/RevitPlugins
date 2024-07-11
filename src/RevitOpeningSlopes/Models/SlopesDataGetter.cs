using System;

using Autodesk.Revit.DB;

namespace RevitOpeningSlopes.Models {
    internal class SlopesDataGetter {
        private readonly RevitRepository _revitRepository;
        public SlopesDataGetter(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
        }

        public SlopeCreationData GetOpeningSlopeCreationData(PluginConfig config, FamilyInstance opening) {

            OpeningHandler openingParameters = new OpeningHandler(_revitRepository, opening);
            double height = openingParameters.GetOpeningHeight();
            double width = openingParameters.GetOpeningWidth();
            double depth = openingParameters.GetOpeningDepth()
                + _revitRepository.ConvertToFeet(double.Parse(config.SlopeFrontOffset));

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

