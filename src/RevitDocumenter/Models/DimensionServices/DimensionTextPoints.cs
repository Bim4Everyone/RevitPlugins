using Autodesk.Revit.DB;

namespace RevitDocumenter.Models.DimensionServices;
/// <summary>
/// Описывает прямоугольную область и контрольные точки текста размера
/// </summary>
internal class DimensionTextPoints {
    public XYZ TopPoint { get; private set; }
    public XYZ RightPoint { get; private set; }
    public XYZ BottomPoint { get; private set; }
    public XYZ LeftPoint { get; private set; }

    public XYZ TopRightCorner { get; private set; }
    public XYZ BottomRightCorner { get; private set; }
    public XYZ BottomLeftCorner { get; private set; }
    public XYZ TopLeftCorner { get; private set; }

    public XYZ TextMiddlePoint { get; private set; }
    public XYZ TextPositionPoint { get; private set; }
    public XYZ TextLineCenterPoint { get; private set; }

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
        TextMiddlePoint = (TopRightCorner + BottomLeftCorner) / 2;

        TopPoint = (TopLeftCorner + TopRightCorner) / 2;
        RightPoint = (TopRightCorner + BottomRightCorner) / 2;
        BottomPoint = (BottomLeftCorner + BottomRightCorner) / 2;
        LeftPoint = (TopLeftCorner + BottomLeftCorner) / 2;
    }

    public void Translate(XYZ translationVector) {
        TopRightCorner += translationVector;
        BottomRightCorner += translationVector;
        BottomLeftCorner += translationVector;
        TopLeftCorner += translationVector;

        TextMiddlePoint += translationVector;
        TextPositionPoint += translationVector;
        TextLineCenterPoint += translationVector;

        TopPoint += translationVector;
        RightPoint += translationVector;
        BottomPoint += translationVector;
        LeftPoint += translationVector;
    }
}
