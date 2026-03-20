using Autodesk.Revit.DB;

namespace RevitDocumenter.Models.DimensionServices;
/// <summary>
/// Описывает прямоугольную область и контрольные точки текста размера
/// </summary>
internal class DimensionTextPoints {
    public XYZ TopRightCorner { get; private set; }
    public XYZ BottomRightCorner { get; private set; }
    public XYZ BottomLeftCorner { get; private set; }
    public XYZ TopLeftCorner { get; private set; }

    public XYZ TextPositionPoint { get; private set; }
    public XYZ TextLineCenterPoint { get; private set; }
    public XYZ TextMiddlePoint { get; private set; }

    public DimensionTextPoints(
        XYZ bottomLeft,
        XYZ bottomRight,
        XYZ textBoxHeightVector,
        XYZ textLineCenterPoint,
        XYZ textPosition) {
        BottomLeftCorner = bottomLeft;
        BottomRightCorner = bottomRight;
        TextLineCenterPoint = textLineCenterPoint;
        TextPositionPoint = textPosition;

        TopLeftCorner = bottomLeft + textBoxHeightVector;
        TopRightCorner = bottomRight + textBoxHeightVector;
        TextMiddlePoint = (BottomLeftCorner + TopRightCorner) / 2;
    }

    public void Translate(XYZ translationVector) {
        TopRightCorner += translationVector;
        BottomRightCorner += translationVector;
        BottomLeftCorner += translationVector;
        TopLeftCorner += translationVector;

        TextPositionPoint += translationVector;
        TextLineCenterPoint += translationVector;
        TextMiddlePoint += translationVector;
    }
}
