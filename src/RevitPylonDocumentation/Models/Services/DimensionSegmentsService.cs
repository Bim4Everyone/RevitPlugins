using System;

using Autodesk.Revit.DB;

namespace RevitPylonDocumentation.Models.Services;
internal class DimensionSegmentsService {
    private readonly View _view;

    // Коэффициенты смещений текста размера
    private readonly double _offsetXY = -0.3;
    private readonly double _offsetZSmall = 0.2;
    private readonly double _offsetZBig = 0.6;

    public DimensionSegmentsService(View view) {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        FindDimensionTextOffsets();
    }
    public XYZ HorizSmallUpDirectDimTextOffset { get; set; }
    public XYZ HorizSmallUpInvertedDimTextOffset { get; set; }
    public XYZ HorizSmallDownDirectDimText { get; set; }
    public XYZ HorizSmallDownInvertedDimTextOffset { get; set; }
    public XYZ HorizBigUpDirectDimTextOffset { get; set; }
    public XYZ HorizBigUpInvertedDimTextOffset { get; set; }

    public XYZ VertSmallUpDirectDimTextOffset { get; set; }
    public XYZ VertSmallUpInvertedDimTextOffset { get; set; }


    private void FindDimensionTextOffsets() {
        // Текст в сегментах размера нужно ставить со смещением от стандартного положения на размерной линии
        // Чтобы он не перекрывал соседние сегменты
        // Т.к. смещение будет зависеть от направления вида, на котором расположен размер, то берем за основу:
        var rightDirection = _view.RightDirection;
        // В зависимости от направления вида рассчитываем смещения
        if(Math.Abs(rightDirection.Y) == 1) {
            HorizSmallUpDirectDimTextOffset = new XYZ(rightDirection.X, rightDirection.Y * _offsetXY, _offsetZSmall);
            HorizSmallUpInvertedDimTextOffset = new XYZ(rightDirection.X, rightDirection.Y * _offsetXY, -_offsetZSmall);

            HorizSmallDownDirectDimText = new XYZ(rightDirection.X, -rightDirection.Y * _offsetXY, _offsetZSmall);
            HorizSmallDownInvertedDimTextOffset = new XYZ(rightDirection.X, -rightDirection.Y * _offsetXY, -_offsetZSmall);

            HorizBigUpDirectDimTextOffset = new XYZ(rightDirection.X, rightDirection.Y * _offsetXY, _offsetZBig);
            HorizBigUpInvertedDimTextOffset = new XYZ(rightDirection.X, rightDirection.Y * _offsetXY, -_offsetZBig);

            VertSmallUpDirectDimTextOffset = new XYZ(rightDirection.X, rightDirection.Y * _offsetXY, 0);
            VertSmallUpInvertedDimTextOffset = new XYZ(rightDirection.X, -rightDirection.Y * _offsetXY, 0);
        } else {
            HorizSmallUpDirectDimTextOffset = new XYZ(rightDirection.X * _offsetXY, rightDirection.Y, _offsetZSmall);
            HorizSmallUpInvertedDimTextOffset = new XYZ(rightDirection.X * _offsetXY, rightDirection.Y, -_offsetZSmall);

            HorizSmallDownDirectDimText = new XYZ(-rightDirection.X * _offsetXY, rightDirection.Y, _offsetZSmall);
            HorizSmallDownInvertedDimTextOffset = new XYZ(-rightDirection.X * _offsetXY, rightDirection.Y, -_offsetZSmall);

            HorizBigUpDirectDimTextOffset = new XYZ(rightDirection.X * _offsetXY, rightDirection.Y, _offsetZBig);
            HorizBigUpInvertedDimTextOffset = new XYZ(rightDirection.X * _offsetXY, rightDirection.Y, -_offsetZBig);

            VertSmallUpDirectDimTextOffset = new XYZ(rightDirection.X * _offsetXY, rightDirection.Y, 0);
            VertSmallUpInvertedDimTextOffset = new XYZ(-rightDirection.X * _offsetXY, rightDirection.Y, 0);
        }

        // Если вид горизонтальный
        if(Math.Abs(_view.ViewDirection.Z) == 1) {
            // Т.к. смещение будет зависеть от направления вида, на котором расположен размер, то берем за основу:
            var upDirection = _view.UpDirection;
            // В зависимости от направления вида рассчитываем смещения
            if(Math.Abs(upDirection.Y) == 1) {
                HorizSmallUpDirectDimTextOffset = new XYZ(upDirection.X, upDirection.Y * _offsetXY, 0);
                HorizSmallUpInvertedDimTextOffset = new XYZ(upDirection.X, -upDirection.Y * _offsetXY, 0);
            } else {
                HorizSmallUpDirectDimTextOffset = new XYZ(upDirection.X * _offsetXY, upDirection.Y, 0);
                HorizSmallUpInvertedDimTextOffset = new XYZ(-upDirection.X * _offsetXY, upDirection.Y, 0);
            }
        }
    }
}
