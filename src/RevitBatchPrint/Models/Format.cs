using Autodesk.Revit.DB;

using System;
using System.Collections.Generic;
using System.Linq;

namespace RevitBatchPrint.Models {
    internal class Format {
        public const string GostPrefix = "B4E_GOST";
        public const string CustomPrefix = "B4E_Custom";

        private const int _defaultFormatTolerance = 5;

        private static readonly List<Format> _gostFormats = new List<Format>() {
            new Format() {Name = $"{GostPrefix}_A0", Width = 841, Height = 1189},
            new Format() {Name = $"{GostPrefix}_A0x2", Width = 1189, Height = 1682},
            new Format() {Name = $"{GostPrefix}_A0x3", Width = 1189, Height = 2523},
            new Format() {Name = $"{GostPrefix}_A1", Width = 594, Height = 841},
            new Format() {Name = $"{GostPrefix}_A1x3", Width = 841, Height = 1783},
            new Format() {Name = $"{GostPrefix}_A1x4", Width = 841, Height = 2378},
            new Format() {Name = $"{GostPrefix}_A2", Width = 420, Height = 594},
            new Format() {Name = $"{GostPrefix}_A2x3", Width = 594, Height = 1261},
            new Format() {Name = $"{GostPrefix}_A2x4", Width = 594, Height = 1682},
            new Format() {Name = $"{GostPrefix}_A2x5", Width = 594, Height = 2102},
            new Format() {Name = $"{GostPrefix}_A3", Width = 297, Height = 420},
            new Format() {Name = $"{GostPrefix}_A3x3", Width = 420, Height = 891},
            new Format() {Name = $"{GostPrefix}_A3x4", Width = 420, Height = 1189},
            new Format() {Name = $"{GostPrefix}_A3x5", Width = 420, Height = 1486},
            new Format() {Name = $"{GostPrefix}_A3x6", Width = 420, Height = 1783},
            new Format() {Name = $"{GostPrefix}_A3x7", Width = 420, Height = 2080},
            new Format() {Name = $"{GostPrefix}_A4", Width = 210, Height = 297},
            new Format() {Name = $"{GostPrefix}_A4x3", Width = 297, Height = 630},
            new Format() {Name = $"{GostPrefix}_A4x4", Width = 297, Height = 841},
            new Format() {Name = $"{GostPrefix}_A4x5", Width = 297, Height = 1051},
            new Format() {Name = $"{GostPrefix}_A4x6", Width = 297, Height = 1261},
            new Format() {Name = $"{GostPrefix}_A4x7", Width = 297, Height = 1471},
            new Format() {Name = $"{GostPrefix}_A4x8", Width = 297, Height = 1682},
            new Format() {Name = $"{GostPrefix}_A4x9", Width = 297, Height = 1892},
        };

        public int Width { get; set; }
        public int Height { get; set; }
        public string Name { get; set; }

        public static Format GetFormat(int width, int height) {
            if(width > height) {
                (height, width) = (width, height);
            }

            return _gostFormats.FirstOrDefault(item =>
                       IsNormalWidth(width, item) && IsNormalHeight(height, item))
                   ?? new Format() {Name = $"{CustomPrefix}_{height}x{width}", Width = width, Height = height};
        }

        private static bool IsNormalHeight(int height, Format item) {
            return IsNormalLength(item.Width, height)
                   || IsNormalLength(item.Height, height);
        }

        private static bool IsNormalWidth(int width, Format item) {
            return (IsNormalLength(item.Width, width)
                    || IsNormalLength(item.Height, width));
        }

        private static bool IsNormalLength(int leftValue, int rightValue, int tolerance = _defaultFormatTolerance) {
            return Math.Abs(leftValue - rightValue) <= tolerance;
        }
    }
}
