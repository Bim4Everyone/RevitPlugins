using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Exceptions;
using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.PointFinders {
    internal class HorizontalPointFinder : IPointFinder {
        private readonly MepCurveClash<Wall> _clash;
        private readonly IValueGetter<DoubleParamValue> _sizeGetter;

        public HorizontalPointFinder(MepCurveClash<Wall> clash, IValueGetter<DoubleParamValue> sizeGetter = null) {
            _clash = clash;
            _sizeGetter = sizeGetter;
        }

        public XYZ GetPoint() {
            var mepLine = _clash.Curve.GetLine();
            //удлинена осевая линия инженерной системы на 5 м в обе стороны
            var elongatedMepLine = Line.CreateBound(mepLine.GetEndPoint(0) - mepLine.Direction * 16.5,
                                      mepLine.GetEndPoint(1) + mepLine.Direction * 16.5);

            //получена линия, идущая вдоль стены и расположенная точно по центру (т.е. линия равноудалена от внутренней и наружной граней стены), и удлинена на 5 м в обе стороны
            var wallLine = _clash.Element.GetСentralLine();
            var elongatedWallLine = Line.CreateBound(wallLine.GetEndPoint(0) - wallLine.Direction * 16.5,
                                     wallLine.GetEndPoint(1) + wallLine.Direction * 16.5);

            //трансформация линии стены в координаты основного файла
            var transformedWallLine = Line.CreateBound(_clash.ElementTransform.OfPoint(elongatedWallLine.GetEndPoint(0)), _clash.ElementTransform.OfPoint(elongatedWallLine.GetEndPoint(1)));
            try {
                //получение точки вставки из уравнения линии 
                if(_sizeGetter != null) {
                    return elongatedMepLine.GetPointFromLineEquation(transformedWallLine) - _clash.Element.Orientation * _clash.Element.Width / 2 - _sizeGetter.GetValue().TValue / 2 * XYZ.BasisZ;
                }
                return elongatedMepLine.GetPointFromLineEquation(transformedWallLine) - _clash.Element.Orientation * _clash.Element.Width / 2;
            } catch {
                throw IntersectionNotFoundException.GetException(_clash.Curve, _clash.Element);
            }
          
        }
    }
}
