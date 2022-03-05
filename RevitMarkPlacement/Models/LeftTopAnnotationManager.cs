
using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitMarkPlacement.Models {
    internal class LeftTopAnnotationManager : AnnotationManager {
        public LeftTopAnnotationManager(RevitRepository revitRepository) : base(revitRepository) {
            _type = _revitRepository.GetAnnotationSymbolType(RevitRepository.TypeTop, RevitRepository.FamilyTop);
        }

        protected override void SetParameters(FamilyInstance annotation, double level, int count, double typicalFloorHeight) {
            base.SetParameters(annotation, level, count, typicalFloorHeight);
            using(Transaction t = _revitRepository.StartTransaction("Поворот аннотации")) {
                _revitRepository.MirrorAnnotation(annotation);
                t.Commit();
            }
        }

        protected override XYZ GetPlacePoint(SpotDimension spot) {
#if D2020 || R2020
            var elevSymbolId = (ElementId) spot.SpotDimensionType.GetParamValueOrDefault(BuiltInParameter.SPOT_ELEV_SYMBOL);
            var elevSymbol = _revitRepository.GetElement(elevSymbolId) as FamilySymbol;
            var width = (double) elevSymbol.GetParamValueOrDefault("Длина полки");
            var height = (double) elevSymbol.GetParamValueOrDefault("Высота полки");
            var textHieght = 1.7 * (double) spot.SpotDimensionType.GetParamValueOrDefault("Размер текста"); // умножение на 1,7 из-за рамки вокруг текста
            var bb = spot.get_BoundingBox(spot.View);
            var scale = spot.View.Scale;
            return new XYZ(bb.Min.X + width * scale, bb.Max.Y, bb.Max.Z - (textHieght + height) * scale);
#else
            return spot.LeaderEndPosition;
#endif
        }
    }
}
