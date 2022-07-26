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

            //получена линия, идущая вдоль стены и расположенная точно по центру (т.е. линия равноудалена от внутренней и наружной граней стены)
            var wallLine = _wall.GetСentralWallLine();

            //трансформация линии стены в координаты основного файла
            var transformedWallLine = Line.CreateBound(_transform.OfPoint(wallLine.GetEndPoint(0)), _transform.OfPoint(wallLine.GetEndPoint(1)));

            //получение точки вставки из уравнения линии 
            return GetPointFromLineEquation(elongatedMepLine, transformedWallLine) - _wall.Orientation * _wall.Width / 2;
        }

        private XYZ GetHorizontalProjectionIntersection(Line mepLine, Line wallLine) {
            var mepLineStart = new XYZ(mepLine.GetEndPoint(0).X, mepLine.GetEndPoint(0).Y, 0);
            var mepLineEnd = new XYZ(mepLine.GetEndPoint(1).X, mepLine.GetEndPoint(1).Y, 0);
            var projectedMepLine = Line.CreateBound(mepLineStart, mepLineEnd);

            var wallLineStart = new XYZ(wallLine.GetEndPoint(0).X, wallLine.GetEndPoint(0).Y, 0);
            var wallLineEnd = new XYZ(wallLine.GetEndPoint(1).X, wallLine.GetEndPoint(1).Y, 0);
            var projectedWallLIne = Line.CreateBound(wallLineStart, wallLineEnd);

            projectedMepLine.Intersect(projectedWallLIne, out IntersectionResultArray result);
            try {
                return result.get_Item(0).XYZPoint;
            } catch {
                throw IntersectionNotFoundException.GetException(_curve, _wall);
            }
        }

        private XYZ GetPointFromLineEquation(Line mepLine, Line wallLine) {
            //Получение проекции точки вставки на плоскость xOy (то есть координаты x и y точки вставки)
            var xy = GetHorizontalProjectionIntersection(mepLine, wallLine);
            double z;
            
            //Подстановка получившихся значений в уравнение прямой в пространстве
            if(Math.Abs(mepLine.Direction.X) > 0.0001) {
                z = (xy.X - mepLine.GetEndPoint(0).X) / mepLine.Direction.X * mepLine.Direction.Z + mepLine.GetEndPoint(0).Z;
            } else {
                z = (xy.Y - mepLine.GetEndPoint(0).Y) / mepLine.Direction.Y * mepLine.Direction.Z + mepLine.GetEndPoint(0).Z;
            }
            return new XYZ(xy.X, xy.Y, z);
        }
    }
}
