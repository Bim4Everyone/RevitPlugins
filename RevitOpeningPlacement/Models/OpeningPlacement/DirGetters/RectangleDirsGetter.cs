using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters;

namespace RevitOpeningPlacement.Models.OpeningPlacement.DirGetters {
    internal class RectangleDirsGetter : IDirectionsGetter {
        private readonly MepCurveWallClash _clash;
        private readonly MepCategory _categoryOption;

        public RectangleDirsGetter(MepCurveWallClash clash, MepCategory categoryOption) {
            _clash = clash;
            _categoryOption = categoryOption;
        }

        public IEnumerable<XYZ> GetDirections() {
            var verticalPlane = _clash.WallTransform.OfPlane(_clash.Wall.GetVerticalNormalPlane());
            var height = new HeightGetter(_clash, _categoryOption).GetParamValue().TValue.TValue;
            var verticalDirs = GetDirs(verticalPlane).Select(item => height * item).ToList();

            var horizontalPlane = _clash.WallTransform.OfPlane(_clash.Wall.GetHorizontalNormalPlane());
            var width = new WidthGetter(_clash, _categoryOption).GetParamValue().TValue.TValue;
            var horizontalDirs = GetDirs(horizontalPlane).Select(item=>item*width).ToList();

            return verticalDirs.SelectMany(item => horizontalDirs.Select(hitem => (hitem + item).Normalize()));
        }

        private IEnumerable<XYZ> GetDirs(Plane plane) {
            return new RoundMepDirsGetter(_clash, plane).GetDirections();
        }
    }
}
