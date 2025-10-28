using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitMarkPlacement.Models.Positions;

internal sealed class RightTopAnnotation : IAnnotationPosition {
    public RightTopAnnotation(XYZ plane, FamilySymbol familySymbol) {
        ViewRightDirection = plane;
        FamilySymbol = familySymbol;
    }

    public bool NeedFlip => false;
    public XYZ ViewRightDirection { get; }
    public FamilySymbol FamilySymbol { get; }

    public XYZ GetPoint(XYZ min, XYZ max, double width, double height, double textHieght, int scale) {
        return new XYZ(
            max.X - ViewRightDirection.X * width * scale,
            max.Y - ViewRightDirection.Y * width * scale,
            max.Z - (textHieght + height) * scale);
    }
}
