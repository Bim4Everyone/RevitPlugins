using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Models.Extensions;

using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;

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

    internal class FittingClash<T> : Clash<FamilyInstance, T> where T : Element {
        public FittingClash(RevitRepository revitRepository, ClashModel clashModel) : base(revitRepository, clashModel) { }
        public override double GetConnectorArea() {
            return Element1.GetMaxConnectorArea();
        }

        public Solid GetRotatedIntersection(IAngleFinder angleFinder) {
            var solid = GetIntersection();
            var zRotates = -angleFinder.GetAngle().Z;
            var transform = Transform.Identity.GetRotationMatrixAroundZ(zRotates);
            return SolidUtils.CreateTransformed(solid, transform);
        }
    }

    internal abstract class Clash<T1, T2> where T1 : Element
                                 where T2 : Element {
        public Clash(RevitRepository revitRepository, ClashModel clashModel) {
            Element1 = (T1) clashModel.MainElement.GetElement(revitRepository.DocInfos);
            Element2 = (T2) clashModel.OtherElement.GetElement(revitRepository.DocInfos);
            Element2Transform = revitRepository.GetTransform(Element2);
        }

        public T1 Element1 { get; set; }
        public T2 Element2 { get; set; }
        public Transform Element2Transform { get; set; }

        public Solid GetIntersection() {
            return Element1.GetSolid().GetIntersection(Element2.GetSolid(), Element2Transform);
        }

        public abstract double GetConnectorArea();
    }

    internal class FittingClashProvider<T> : IClashProvider<FamilyInstance, T> where T : Element {
        public Clash<FamilyInstance, T> GetClash(RevitRepository revitRepository, ClashModel model) {
            return new FittingClash<T>(revitRepository, model);
        }
    }

    internal class MepCurveClashProvider<T> : IClashProvider<MEPCurve, T> where T : Element {
        public Clash<MEPCurve, T> GetClash(RevitRepository revitRepository, ClashModel model) {
            return new MepCurveClash<T>(revitRepository, model);
        }
    }
}
