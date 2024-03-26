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
            SlopeCreationData slopeData = null;
            foreach(FamilyInstance opening in openings) {
                //XYZ topXYZ = _openingTopXYZGetter.GetOpeningTopXYZ(opening);
                //if(topXYZ == null) {
                //    continue;
                //}
                OpeningHandler openingParameters = new OpeningHandler(_revitRepository, opening);
                //double height = GetOpeningHeight(opening);
                double height = openingParameters.OpeningHeight;
                if(height <= 0) {
                    continue;
                }

                //XYZ g = GetVerticalCenterPoint(opening);
                //GetDepthPoint(opening);
                //double width = GetOpeningWidth(opening);
                double width = openingParameters.OpeningWidth;
                if(width <= 0) {
                    continue;
                }
                //double depth = GetOpeningDepth(opening);
                //XYZ center = GetVerticalCenterPoint(opening);
                double depth = openingParameters.OpeningDepth;
                XYZ center = openingParameters.OpeningCenterPoint;
                //XYZ center = _openingCenterXYZGetter.GetOpeningCenter(opening);
                //XYZ frontPoint = _openingFrontPointGetter.GetFrontPoint(opening);
                slopeData = new SlopeCreationData(_revitRepository.Document) {
                    Height = height,
                    Width = width,
                    Depth = depth,
                    Center = center,
                    SlopeTypeId = config.SlopeTypeId
                };
                slopeCreationData.Add(slopeData);
            }
            return slopeCreationData;
        }
    }
}

