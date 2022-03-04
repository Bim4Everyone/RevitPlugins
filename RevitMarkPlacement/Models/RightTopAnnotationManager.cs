
using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitMarkPlacement.Models {
    internal class RightTopAnnotationManager : AnnotationManager {
        private FamilySymbol _type;

        public RightTopAnnotationManager(RevitRepository revitRepository) : base(revitRepository) {
            _type = _revitRepository.GetAnnotationSymbolType("Вверх", "ТипАн_Отметка_ТипЭт_Разрез_Вверх");
        }

        protected override void UpdatePlacement(SpotDimension spot, AnnotationSymbol annotation) {
            base.UpdatePlacement(spot, annotation);
        }

        protected override void UpdateValues(SpotDimension spot, AnnotationSymbol annotation, int floorCount, double floorHeight) {
            base.UpdateValues(spot, annotation, floorCount, floorHeight);

        }

        protected override void Place(SpotDimension spot, int floorCount, double floorHeight) {
            base.Place(spot, floorCount, floorHeight);
        }

        private protected override XYZ GetPlacePoint(SpotDimension spot) {
#if D2020 || R2020
            var elevSymbolId = (ElementId) spot.SpotDimensionType.GetParamValueOrDefault(BuiltInParameter.SPOT_ELEV_SYMBOL);
            var elevSymbol = _revitRepository.GetElement(elevSymbolId) as FamilySymbol;
            var width = (double) elevSymbol.GetParamValueOrDefault("Длина полки");
            var height = (double) elevSymbol.GetParamValueOrDefault("Высота полки");
            var textHieght = 1.7 *(double) spot.SpotDimensionType.GetParamValueOrDefault("Размер текста"); // умножение на 1,7 из-за рамки вокруг текста
            var bb = spot.get_BoundingBox(spot.View);
            var scale = spot.View.Scale;
            return new XYZ(bb.Max.X - width * scale, bb.Max.Y, bb.Max.Z - (textHieght + height) * scale);
#else
            return spot.LeaderEndPosition;
#endif
        }
    }
}
