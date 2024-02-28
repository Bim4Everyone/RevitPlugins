using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitOpeningSlopes.Models {
    internal class SlopesParams {
        private readonly RevitRepository _revitRepository;
        private readonly OpeningParams _openingParams;
        public SlopesParams(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
            _openingParams = new OpeningParams(revitRepository);
        }
        public void SetSlopeParams(FamilyInstance slope, FamilyInstance opening) {
            Parameter heightParam = ElementExtensions.GetParam(slope, "Высота");
            heightParam.Set(_openingParams.GetOpeningHeight(opening));
        }
    }
}
