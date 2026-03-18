using Autodesk.Revit.DB;

namespace RevitDocumenter.Models.DimensionServices;
/// <summary>
/// Описывает прямоугольную область и контрольные точки текста размера
/// </summary>
internal class DimensionTextPoints {
    public XYZ BottomLeftCorner { get; private set; }
    public XYZ BottomRightCorner { get; private set; }
    public XYZ TopLeftCorner { get; private set; }
    public XYZ TopRightCorner { get; private set; }
    public XYZ DimensionCenterPoint { get; private set; }
    public XYZ TextPositionPoint { get; private set; }
    public double Height { get; }

    public DimensionTextPoints(
        XYZ bottomLeft,
        XYZ bottomRight,
        XYZ upVector,
        double height,
        XYZ centerPoint,
        XYZ textPosition) {
        BottomLeftCorner = bottomLeft;
        BottomRightCorner = bottomRight;
        DimensionCenterPoint = centerPoint;
        TextPositionPoint = textPosition;
        Height = height;

        TopLeftCorner = bottomLeft + upVector * height;
        TopRightCorner = bottomRight + upVector * height;
    }

    public void Translate(XYZ translationVector) {
        BottomLeftCorner += translationVector;
        BottomRightCorner += translationVector;
        TopLeftCorner += translationVector;
        TopRightCorner += translationVector;
    }
}
