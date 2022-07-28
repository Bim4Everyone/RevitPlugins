using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitOpeningPlacement.Models.OpeningPlacement {
    internal class MepCurveWallClash {
        public MepCurveWallClash(MEPCurve curve, Wall wall, Transform transform) {
            Curve = curve;
            Wall = wall;
            WallTransform = transform;
        }

        public MEPCurve Curve { get; set; }

        public Wall Wall { get; set; }

        public Transform WallTransform { get; set; }

        public Line GetTransformedMepLine() {
            var mepLine = (Line) ((LocationCurve) Curve.Location).Curve;

            //примерно на 5 м с обеих сторон удлинена осевая линия инженерной системы
            var elongatedMepLine = Line.CreateBound(mepLine.GetEndPoint(0) - mepLine.Direction * 16.5,
                                                    mepLine.GetEndPoint(1) + mepLine.Direction * 16.5);

            //трансформация осевой линии инженерной системы в систему координат файла со стеной
            var inversedTransform = WallTransform.Inverse.Multiply(Transform.Identity);
            return Line.CreateBound(inversedTransform.OfPoint(elongatedMepLine.GetEndPoint(0)),
                                                       inversedTransform.OfPoint(elongatedMepLine.GetEndPoint(1)));
        }
    }
}
