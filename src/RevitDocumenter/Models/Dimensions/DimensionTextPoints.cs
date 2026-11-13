using Autodesk.Revit.DB;

namespace RevitDocumenter.Models.Dimensions;
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
        BottomLeftCorner = bottomLeft.ThrowIfNull();
        BottomRightCorner = bottomRight.ThrowIfNull();
        TextLineCenterPoint = textLineCenterPoint.ThrowIfNull();
        TextPositionPoint = textPosition.ThrowIfNull();

        TopLeftCorner = bottomLeft + textBoxHeightVector;
        TopRightCorner = bottomRight + textBoxHeightVector;
        TextMiddlePoint = (BottomLeftCorner + TopRightCorner) / 2;
    }

    public DimensionTextPoints(
        XYZ topRightCorner,
        XYZ bottomRightCorner,
        XYZ bottomLeftCorner,
        XYZ topLeftCorner,
        XYZ textPositionPoint,
        XYZ textLineCenterPoint,
        XYZ textMiddlePoint) {
        TopRightCorner = topRightCorner.ThrowIfNull();
        BottomRightCorner = bottomRightCorner.ThrowIfNull();
        BottomLeftCorner = bottomLeftCorner.ThrowIfNull();
        TopLeftCorner = topLeftCorner.ThrowIfNull();
        TextPositionPoint = textPositionPoint.ThrowIfNull();
        TextLineCenterPoint = textLineCenterPoint.ThrowIfNull();
        TextMiddlePoint = textMiddlePoint.ThrowIfNull();
    }

    public void Translate(XYZ translationVector) {
        TopRightCorner += translationVector.ThrowIfNull();
        BottomRightCorner += translationVector;
        BottomLeftCorner += translationVector;
        TopLeftCorner += translationVector;

        TextPositionPoint += translationVector;
        TextLineCenterPoint += translationVector;
        TextMiddlePoint += translationVector;
    }

    public DimensionTextPoints Clone() {
        return new DimensionTextPoints(
            new XYZ(TopRightCorner.X, TopRightCorner.Y, TopRightCorner.Z),
            new XYZ(BottomRightCorner.X, BottomRightCorner.Y, BottomRightCorner.Z),
            new XYZ(BottomLeftCorner.X, BottomLeftCorner.Y, BottomLeftCorner.Z),
            new XYZ(TopLeftCorner.X, TopLeftCorner.Y, TopLeftCorner.Z),
            new XYZ(TextPositionPoint.X, TextPositionPoint.Y, TextPositionPoint.Z),
            new XYZ(TextLineCenterPoint.X, TextLineCenterPoint.Y, TextLineCenterPoint.Z),
            new XYZ(TextMiddlePoint.X, TextMiddlePoint.Y, TextMiddlePoint.Z)
        );
    }
}
