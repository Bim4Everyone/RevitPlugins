using Autodesk.Revit.DB;

namespace RevitOpeningSlopes.Models {
    internal class OverallPoints {
        public XYZ Left { get; private set; }
        public XYZ Right { get; private set; }
        public XYZ Top { get; private set; }
        public XYZ Bottom { get; private set; }
        public OverallPoints(XYZ left, XYZ right, XYZ top, XYZ bottom) {
            Left = left;
            Right = right;
            Top = top;
            Bottom = bottom;
        }
    }
}
