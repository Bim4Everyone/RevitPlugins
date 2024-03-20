using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitOpeningSlopes.Services.ValueGetter;

namespace RevitOpeningSlopes.Models {
    internal class SlopesDataGetter {
        private readonly RevitRepository _revitRepository;
        private readonly OpeningTopXYZGetter _openingTopXYZGetter;
        private readonly OpeningCenterXYZGetter _openingCenterXYZGetter;
        private readonly OpeningRightXYZGetter _openingRightXYZGetter;
        private readonly OpeningFrontPointGetter _openingFrontPointGetter;
        private readonly OpeningHeightGetter _openingHeightGetter;
        private readonly OpeningWidthGetter _openingWidthGetter;
        private readonly LinesFromOpening _linesFromOpening;
        private readonly SlopeParams _slopesParams;

        public SlopesDataGetter(
            RevitRepository revitRepository,
            OpeningTopXYZGetter openingTopXYZGetter,
            OpeningCenterXYZGetter openingCenterXYZGetter,
            OpeningRightXYZGetter openingRightXYZGetter,
            OpeningFrontPointGetter openingFrontPointGetter,
            OpeningHeightGetter openingHeightGetter,
            OpeningWidthGetter openingWidthGetter) {

            _revitRepository = revitRepository;
            _openingTopXYZGetter = openingTopXYZGetter
                ?? throw new System.ArgumentNullException(nameof(openingTopXYZGetter));
            _openingCenterXYZGetter = openingCenterXYZGetter
                ?? throw new System.ArgumentNullException(nameof(openingCenterXYZGetter));
            _openingRightXYZGetter = openingRightXYZGetter
                ?? throw new System.ArgumentNullException(nameof(openingRightXYZGetter));
            _openingFrontPointGetter = openingFrontPointGetter
                ?? throw new System.ArgumentNullException(nameof(openingFrontPointGetter));
            _openingHeightGetter = openingHeightGetter
                ?? throw new System.ArgumentNullException(nameof(openingHeightGetter));
            _openingWidthGetter = openingWidthGetter
                ?? throw new System.ArgumentNullException(nameof(openingWidthGetter));
            _linesFromOpening = new LinesFromOpening(revitRepository);
            _slopesParams = new SlopeParams(revitRepository);
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

                double height = _openingHeightGetter.GetOpeningHeight(opening);
                if(height <= 0) {
                    continue;
                }

                double width = _openingWidthGetter.GetOpeningWidth(opening);
                if(width <= 0) {
                    continue;
                }

                XYZ center = _openingCenterXYZGetter.GetOpeningCenter(opening);
                XYZ frontPoint = _openingFrontPointGetter.GetFrontPoint(opening);
                slopeData = new SlopeCreationData(_revitRepository.Document) {
                    Height = height,
                    Width = width,
                    Center = center,
                    SlopeTypeId = config.SlopeTypeId
                };
                slopeCreationData.Add(slopeData);
            }
            return slopeCreationData;
        }
    }
}

