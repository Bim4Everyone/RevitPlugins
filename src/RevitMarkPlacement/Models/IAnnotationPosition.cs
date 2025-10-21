using System;

using Autodesk.Revit.DB;

namespace RevitMarkPlacement.Models;

internal interface IAnnotationPosition {
    bool NeedFlip { get; }
    XYZ ViewRightDirection { get; }
    FamilySymbol FamilySymbol { get; }
    XYZ GetPoint(XYZ min, XYZ max, double width, double height, double textHeight, int scale);
}
