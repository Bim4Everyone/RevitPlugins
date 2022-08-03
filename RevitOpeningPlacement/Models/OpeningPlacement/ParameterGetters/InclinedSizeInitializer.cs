
using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.OpeningPlacement.DirGetters;
using RevitOpeningPlacement.Models.OpeningPlacement.Projectors;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters {
    internal class InclinedSizeInitializer {
        private readonly MepCurveWallClash _clash;
        private readonly MepCategory _mepCategory;
        private readonly Line _wallLine;

        public InclinedSizeInitializer(MepCurveWallClash clash, MepCategory mepCategory) {
            _clash = clash;
            _mepCategory = mepCategory;
            _wallLine = _clash.Wall.GetСentralLine()
           .GetTransformedLine(_clash.WallTransform);
        }

        public InclinedSizeGetter GetRoundMepHeightGetter() {
            var heightProjector = new PlaneProjector(_wallLine.Origin, XYZ.BasisZ, _clash.WallTransform.OfVector(_clash.Wall.Orientation));
            var heightDirsGetter = new RoundMepDirsGetter(_clash, heightProjector);
            return new InclinedSizeGetter(_clash, new DiameterGetter(_clash, _mepCategory), heightProjector, heightDirsGetter, RevitRepository.OpeningHeight);
        }

        public InclinedSizeGetter GetRoundMepWidthGetter() {
            var widthProjector = new PlaneProjector(_wallLine.Origin, _wallLine.Direction, _clash.WallTransform.OfVector(_clash.Wall.Orientation));
            var widthDirsGetters = new RoundMepDirsGetter(_clash, widthProjector);
            return new InclinedSizeGetter(_clash, new DiameterGetter(_clash, _mepCategory), widthProjector, widthDirsGetters, RevitRepository.OpeningWidth);
        }
        public InclinedSizeGetter GetRectangleMepHeightGetter() {
            var heightProjector = new PlaneProjector(_wallLine.Origin, XYZ.BasisZ, _clash.WallTransform.OfVector(_clash.Wall.Orientation));
            var heightDirsGetter = new RoundMepDirsGetter(_clash, heightProjector);
            return new InclinedSizeGetter(_clash, new HeightGetter(_clash, _mepCategory), heightProjector, heightDirsGetter, RevitRepository.OpeningHeight);
        }

        public InclinedSizeGetter GetRectangleMepWidthGetter() {
            var widthProjector = new PlaneProjector(_wallLine.Origin, _wallLine.Direction, _clash.WallTransform.OfVector(_clash.Wall.Orientation));
            var widthDirsGetters = new RoundMepDirsGetter(_clash, widthProjector);
            return new InclinedSizeGetter(_clash, new WidthGetter(_clash, _mepCategory), widthProjector, widthDirsGetters, RevitRepository.OpeningWidth);
        }
    }
}
