using System;

using Autodesk.Revit.DB;

namespace RevitOpeningSlopes.Models {
    internal class SlopesDataGetter {
        private readonly RevitRepository _revitRepository;


        public SlopesDataGetter(RevitRepository revitRepository) {

            _revitRepository = revitRepository;
        }

        //public IList<SlopeCreationData> GetOpeningSlopesCreationData(PluginConfig config,
        //    out ICollection<ElementId> notProcessedOpenings) {
        //    ICollection<FamilyInstance> openings = _revitRepository
        //            .GetWindows(config.WindowsGetterMode);

        //    List<SlopeCreationData> slopeCreationData = new List<SlopeCreationData>();
        //    notProcessedOpenings = new List<ElementId>();

        //    foreach(FamilyInstance opening in openings) {
        //        try {
        //            OpeningHandler openingParameters = new OpeningHandler(_revitRepository, opening);
        //            double height = openingParameters.GetOpeningHeight();
        //            double width = openingParameters.GetOpeningWidth();
        //            double depth = openingParameters.GetOpeningDepth()
        //                + _revitRepository.ConvertToFeet(double.Parse(config.SlopeFrontOffset));

        //            XYZ center = openingParameters.GetVerticalCenterPoint();
        //            double rotationAngle = openingParameters.GetRotationAngle();

        //            if(height <= 0 || width <= 0 || depth <= 0 || center == null) {
        //                continue;
        //            }

        //            SlopeCreationData slopeData = new SlopeCreationData(_revitRepository.Document) {
        //                Height = height,
        //                Width = width,
        //                Depth = depth,
        //                Center = center,
        //                SlopeTypeId = config.SlopeTypeId,
        //                RotationRadiansAngle = rotationAngle
        //            };
        //            slopeCreationData.Add(slopeData);
        //        } catch(OpeningNullSolidException) {
        //            notProcessedOpenings.Add(opening.Id);
        //            throw new OpeningNullSolidException($"Нельзя создать с таким id {opening.Id}");
        //            //continue;
        //        }



        //    }
        //    return slopeCreationData;
        //}
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


            SlopeCreationData slopeData = new SlopeCreationData(_revitRepository.Document) {
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

