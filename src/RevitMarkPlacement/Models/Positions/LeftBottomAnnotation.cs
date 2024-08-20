using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitMarkPlacement.Models {
    internal class LeftBottomAnnotation : IAnnotationPosition {

        public LeftBottomAnnotation(IEnumerable<FamilySymbol> symbols, XYZ plane) {
            FamilySymbol = symbols.FirstOrDefault(item => item.Name.Equals(RevitRepository.TypeBottom, StringComparison.CurrentCultureIgnoreCase));
            ViewRightDirection = plane;
        }
        public bool NeedFlip => true;

        public FamilySymbol FamilySymbol { get; }

        public XYZ ViewRightDirection { get; }

        public XYZ GetPoint(XYZ min, XYZ max, double width, double height, double textHieght, int scale) {
            return new XYZ(min.X + ViewRightDirection.X * width * scale,
                min.Y + ViewRightDirection.Y * width * scale,
                min.Z + height * scale);
        }
    }
}
