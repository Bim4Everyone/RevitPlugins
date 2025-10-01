using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitPylonDocumentation.Models.PluginOptions;

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


    /// <summary>
    /// Изменяет крайние сегменты размера
    /// </summary>
    public void EditEdgeDimensionSegments(Dimension dimension, XYZ leftDimTextOffset, XYZ rightDimTextOffset) {
        if(dimension.NumberOfSegments < 3) { return; }

        // Создаем коллекцию опций изменений размера
        var defOption = new DimensionSegmentOption(false);

        var dimSegmentOpts = new List<DimensionSegmentOption>();
        dimSegmentOpts.Add(new(true, "", leftDimTextOffset));
        dimSegmentOpts.AddRange(Enumerable.Repeat(defOption, dimension.NumberOfSegments - 2));
        dimSegmentOpts.Add(new(true, "", rightDimTextOffset));
        // Применяем изменения
        ApplySegmentsModification(dimension, dimSegmentOpts);
    }


    public void ApplySegmentsModification(Dimension dimension, List<DimensionSegmentOption> dimSegmentOpts) {
        if(dimension.NumberOfSegments < dimSegmentOpts.Count) { return; }

        // Применяем опции изменений сегментов размера
        var dimensionSegments = dimension.Segments;
        for(int i = 0; i < dimensionSegments.Size; i++) {
            var dimSegmentMod = dimSegmentOpts[i];

            if(dimSegmentMod.ModificationNeeded) {
                var segment = dimensionSegments.get_Item(i);
                segment.Prefix = dimSegmentMod.Prefix;

                var oldTextPosition = segment.TextPosition;
                segment.TextPosition = oldTextPosition + dimSegmentMod.TextOffset;
            }
        }
    }
}
