using Autodesk.Revit.DB;

using RevitClashDetective.Models.Clashes;

using RevitOpeningPlacement.Models.Extensions;

namespace RevitOpeningPlacement.Models.OpeningPlacement {
    internal class MepCurveClash<T> : Clash<MEPCurve, T> where T : Element {
        public MepCurveClash(RevitRepository revitRepository, ClashModel clashModel) : base(revitRepository, clashModel) { }

        public Line GetTransformedMepLine() {
            var mepLine = Element1.GetLine();

            //примерно на 5 м с обеих сторон удлинена осевая линия инженерной системы
            var elongatedMepLine = Line.CreateBound(mepLine.GetEndPoint(0) - mepLine.Direction * 16.5,
                                                    mepLine.GetEndPoint(1) + mepLine.Direction * 16.5);

            //трансформация осевой линии инженерной системы в систему координат файла со стеной
            var inversedTransform = Element2Transform.Inverse.Multiply(Transform.Identity);
            return elongatedMepLine.GetTransformedLine(inversedTransform);
        }

        public override double GetConnectorArea() {
            return Element1.GetConnectorArea();
        }
    }
}
