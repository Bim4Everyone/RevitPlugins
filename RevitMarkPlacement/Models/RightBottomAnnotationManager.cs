
using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitMarkPlacement.Models {
    internal class RightBottomAnnotationManager : AnnotationManager {
        public RightBottomAnnotationManager(RevitRepository revitRepository) : base(revitRepository) {
            _type = _revitRepository.GetAnnotationSymbolType(RevitRepository.TypeBottom, RevitRepository.FamilyBottom);
        }

        protected override XYZ GetPlacePoint(SpotDimension spot) {
#if D2020 || R2020
            var elevSymbolId = (ElementId) spot.SpotDimensionType.GetParamValueOrDefault(BuiltInParameter.SPOT_ELEV_SYMBOL);
            var elevSymbol = _revitRepository.GetElement(elevSymbolId) as FamilySymbol;
            var width = (double) elevSymbol.GetParamValueOrDefault("Длина полки");
            var height = (double) elevSymbol.GetParamValueOrDefault("Высота полки");
            var bb = spot.get_BoundingBox(spot.View);
            var scale = spot.View.Scale;
            return new XYZ(bb.Max.X - width * scale, bb.Min.Y, bb.Min.Z + height * scale);
#else
            return spot.LeaderEndPosition;
#endif
        }
    }
}
