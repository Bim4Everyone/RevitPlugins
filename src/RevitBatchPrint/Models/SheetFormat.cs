using System;
using System.Collections.Generic;
using System.Linq;

namespace RevitBatchPrint.Models;

internal class SheetFormat {
    public const string GostPrefix = "B4E_GOST";
    public const string CustomPrefix = "B4E_Custom";

    // ЕСКД ГОСТ 2.301
    // Предельные отклонения размеров сторон форматов (в мм):
    // --------------------------------------------------------
    // Размер стороны (мм)     | Предельное отклонение (мм)
    // ------------------------|------------------------------
    // До 150                  | ±1.5
    // Св. 150 до 600          | ±2.0
    // Св. 600                 | ±3.0
    // не стал делать четко по стандарту, взял значение чуть больше :)
    private const int _defaultFormatTolerance = 5;

    // ЕСКД ГОСТ 2.301 (перечислены все форматы из ГОСТ)
    private static readonly IReadOnlyCollection<SheetFormat> _gostFormats = [
        new() {Name = $"{GostPrefix}_A0", WidthMm = 841, HeightMm = 1189},
        new() {Name = $"{GostPrefix}_A0x2", WidthMm = 1189, HeightMm = 1682},
        new() {Name = $"{GostPrefix}_A0x3", WidthMm = 1189, HeightMm = 2523},
        new() {Name = $"{GostPrefix}_A1", WidthMm = 594, HeightMm = 841},
        new() {Name = $"{GostPrefix}_A1x3", WidthMm = 841, HeightMm = 1783},
        new() {Name = $"{GostPrefix}_A1x4", WidthMm = 841, HeightMm = 2378},
        new() {Name = $"{GostPrefix}_A2", WidthMm = 420, HeightMm = 594},
        new() {Name = $"{GostPrefix}_A2x3", WidthMm = 594, HeightMm = 1261},
        new() {Name = $"{GostPrefix}_A2x4", WidthMm = 594, HeightMm = 1682},
        new() {Name = $"{GostPrefix}_A2x5", WidthMm = 594, HeightMm = 2102},
        new() {Name = $"{GostPrefix}_A3", WidthMm = 297, HeightMm = 420},
        new() {Name = $"{GostPrefix}_A3x3", WidthMm = 420, HeightMm = 891},
        new() {Name = $"{GostPrefix}_A3x4", WidthMm = 420, HeightMm = 1189},
        new() {Name = $"{GostPrefix}_A3x5", WidthMm = 420, HeightMm = 1486},
        new() {Name = $"{GostPrefix}_A3x6", WidthMm = 420, HeightMm = 1783},
        new() {Name = $"{GostPrefix}_A3x7", WidthMm = 420, HeightMm = 2080},
        new() {Name = $"{GostPrefix}_A4", WidthMm = 210, HeightMm = 297},
        new() {Name = $"{GostPrefix}_A4x3", WidthMm = 297, HeightMm = 630},
        new() {Name = $"{GostPrefix}_A4x4", WidthMm = 297, HeightMm = 841},
        new() {Name = $"{GostPrefix}_A4x5", WidthMm = 297, HeightMm = 1051},
        new() {Name = $"{GostPrefix}_A4x6", WidthMm = 297, HeightMm = 1261},
        new() {Name = $"{GostPrefix}_A4x7", WidthMm = 297, HeightMm = 1471},
        new() {Name = $"{GostPrefix}_A4x8", WidthMm = 297, HeightMm = 1682},
        new() {Name = $"{GostPrefix}_A4x9", WidthMm = 297, HeightMm = 1892}
    ];

    public string Name { get; private set; }

    public int WidthMm { get; private set; }
    public int HeightMm { get; private set; }

    public static SheetFormat GetFormat(int width, int height) {
        if(width > height) {
            (height, width) = (width, height);
        }

        return _gostFormats.FirstOrDefault(item =>
                   IsNormalWidth(width, item) && IsNormalHeight(height, item))
               ?? new SheetFormat {Name = $"{CustomPrefix}_{height}x{width}", WidthMm = width, HeightMm = height};
    }

    private static bool IsNormalHeight(int height, SheetFormat item) {
        return IsNormalLength(item.WidthMm, height)
               || IsNormalLength(item.HeightMm, height);
    }

    private static bool IsNormalWidth(int width, SheetFormat item) {
        return IsNormalLength(item.WidthMm, width)
               || IsNormalLength(item.HeightMm, width);
    }

    private static bool IsNormalLength(int leftValue, int rightValue, int tolerance = _defaultFormatTolerance) {
        return Math.Abs(leftValue - rightValue) <= tolerance;
    }
}
