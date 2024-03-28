using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitOpeningSlopes.Models {
    internal class SlopesDataGetter {
        private readonly RevitRepository _revitRepository;


        public SlopesDataGetter(RevitRepository revitRepository) {

            _revitRepository = revitRepository;
        }

        public IList<SlopeCreationData> GetSlopeCreationData(PluginConfig config) {
            ICollection<FamilyInstance> openings = _revitRepository
                    .GetWindows(config.WindowsGetterMode);

            List<SlopeCreationData> slopeCreationData = new List<SlopeCreationData>();
            foreach(FamilyInstance opening in openings) {

                OpeningHandler openingParameters = new OpeningHandler(_revitRepository, opening);

                double height = openingParameters.OpeningHeight;
                double width = openingParameters.OpeningWidth;
                double depth = openingParameters.OpeningDepth
                    + _revitRepository.ConvertToFeet(double.Parse(config.SlopeFrontOffset));

                XYZ center = openingParameters.OpeningCenterPoint;
                double rotationAngle = openingParameters.RotationAngle;

                if(height <= 0 || width <= 0 || depth <= 0 || center == null) {
                    continue;
                }


                SlopeCreationData slopeData = new SlopeCreationData(_revitRepository.Document) {
                    Height = height,
                    Width = width,
                    Depth = depth,
                    Center = center,
                    SlopeTypeId = config.SlopeTypeId,
                    RotationRadiansAngle = rotationAngle
                };
                slopeCreationData.Add(slopeData);
            }
            return slopeCreationData;
        }
    }
}

