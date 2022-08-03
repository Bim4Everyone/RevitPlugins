﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Clashes;

using RevitOpeningPlacement.Models.Extensions;

namespace RevitOpeningPlacement.Models.OpeningPlacement {
    internal class MepCurveWallClash {
        public MepCurveWallClash(RevitRepository revitRepository, ClashModel clashModel) {
            Curve = (MEPCurve) clashModel.MainElement.GetElement(revitRepository.DocInfos);
            Wall = (Wall) clashModel.OtherElement.GetElement(revitRepository.DocInfos);
            WallTransform = revitRepository.GetTransform(Wall);
        }

        public MEPCurve Curve { get; set; }

        public Wall Wall { get; set; }

        public Transform WallTransform { get; set; }

        public Line GetTransformedMepLine() {
            var mepLine = Curve.GetLine();

            //примерно на 5 м с обеих сторон удлинена осевая линия инженерной системы
            var elongatedMepLine = Line.CreateBound(mepLine.GetEndPoint(0) - mepLine.Direction * 16.5,
                                                    mepLine.GetEndPoint(1) + mepLine.Direction * 16.5);

            //трансформация осевой линии инженерной системы в систему координат файла со стеной
            var inversedTransform = WallTransform.Inverse.Multiply(Transform.Identity);
            return elongatedMepLine.GetTransformedLine(inversedTransform);
        }
    }
}
