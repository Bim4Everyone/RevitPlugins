using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Exceptions;
using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement {
    internal class HorizontalPointFinder : IPointFinder {
        private readonly MEPCurve _curve;
        private readonly Wall _wall;
        private readonly Transform _transform;

        public HorizontalPointFinder(MEPCurve curve, Wall wall, Transform transform) {
            _curve = curve;
            _wall = wall;
            _transform = transform;
        }

        public XYZ GetPoint() {

            var mepLine = (Line) ((LocationCurve) _curve.Location).Curve;
            //удлинена осевая линия инженерной системы на 5 м в обе стороны
            var elongatedMepLine = Line.CreateBound(mepLine.GetEndPoint(0) - mepLine.Direction * 16.5,
                                      mepLine.GetEndPoint(1) + mepLine.Direction * 16.5);

            //получена линия, идущая вдоль стены и расположенная точно по центру (т.е. линия равноудалена от внутренней и наружной граней стены), и удлинена на 5 м в обе стороны
            var wallLine = _wall.GetСentralWallLine();
            var elongatedWallLine = Line.CreateBound(wallLine.GetEndPoint(0) - wallLine.Direction * 16.5,
                                     wallLine.GetEndPoint(1) + wallLine.Direction * 16.5);

            //трансформация линии стены в координаты основного файла
            var transformedWallLine = Line.CreateBound(_transform.OfPoint(elongatedWallLine.GetEndPoint(0)), _transform.OfPoint(elongatedWallLine.GetEndPoint(1)));

            //получение точки вставки из уравнения линии 
            return elongatedMepLine.GetPointFromLineEquation(transformedWallLine) - _wall.Orientation * _wall.Width / 2;
        }
    }
}
