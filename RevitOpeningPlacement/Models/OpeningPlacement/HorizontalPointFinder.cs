using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement {
    internal class HorizontalPointFinder : IPointFinder {
        private readonly MEPCurve _curve;
        private readonly Wall _wall;

        public HorizontalPointFinder(MEPCurve curve, Wall wall) {
            _curve = curve;
            _wall = wall;
        }

        public XYZ GetPoint() {
            var mepLine = (Line) ((LocationCurve) _curve.Location).Curve;
            var wallLine = _wall.GetСentralWallLine();

            var raisedWallLine = Line.CreateBound(new XYZ(wallLine.GetEndPoint(0).X, wallLine.GetEndPoint(0).Y, mepLine.GetEndPoint(0).Z),
                                                  new XYZ(wallLine.GetEndPoint(1).X, wallLine.GetEndPoint(1).Y, mepLine.GetEndPoint(0).Z));
            var elongatedMepLine = Line.CreateBound(mepLine.GetEndPoint(0) - mepLine.Direction * _wall.Width / 2,
                                                  mepLine.GetEndPoint(1) + mepLine.Direction * _wall.Width / 2);

            elongatedMepLine.Intersect(raisedWallLine, out IntersectionResultArray results);
            try {
                return results.get_Item(0).XYZPoint - _wall.Orientation * _wall.Width / 2;
            } catch {
                throw new Exception($"Не удалось найти точку пересечения между элементами \"{_curve.Id}\" и \"{_wall.Id}\"");
            }
        }
    }
}
