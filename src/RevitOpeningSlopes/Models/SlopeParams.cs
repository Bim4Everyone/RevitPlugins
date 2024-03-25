using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitOpeningSlopes.Models {
    internal class SlopeParams {
        private readonly RevitRepository _revitRepository;
        public SlopeParams(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
        }
        public void SetSlopeParams(FamilyInstance slope, SlopeCreationData slopeCreationData) {
            Parameter heightParam = ElementExtensions.GetParam(slope, "Высота");
            heightParam.Set(slopeCreationData.Height);

            Parameter widthParam = ElementExtensions.GetParam(slope, "Ширина");
            widthParam.Set(slopeCreationData.Width);

            Parameter depthParam = ElementExtensions.GetParam(slope, "Ширина откоса");
            depthParam.Set(slopeCreationData.Depth);
        }
    }
}
