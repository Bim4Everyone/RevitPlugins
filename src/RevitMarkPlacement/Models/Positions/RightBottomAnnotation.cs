using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitMarkPlacement.Models.Positions;

internal sealed class RightBottomAnnotation : IAnnotationPosition {
    public RightBottomAnnotation(XYZ plane, IEnumerable<FamilySymbol> symbols) {
        ViewRightDirection = plane;
        FamilySymbol = symbols.FirstOrDefault(item =>
            item.Name.Equals(
                RevitRepository.TypeBottom,
                StringComparison.CurrentCultureIgnoreCase));
    }

    public bool NeedFlip => false;
    public XYZ ViewRightDirection { get; }
    public FamilySymbol FamilySymbol { get; }

    public XYZ GetPoint(XYZ min, XYZ max, double width, double height, double textHieght, int scale) {
        return new XYZ(
            max.X - ViewRightDirection.X * width * scale,
            max.Y - ViewRightDirection.Y * width * scale,
            min.Z + height * scale);
    }
}
