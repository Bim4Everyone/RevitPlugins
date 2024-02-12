
using Autodesk.Revit.DB;

namespace RevitMarkPlacement.Models {
    internal interface IAnnotationPosition {
        bool NeedFlip { get; }
        XYZ ViewRightDirection { get; }
        FamilySymbol FamilySymbol { get; }
        XYZ GetPoint(XYZ min, XYZ max, double width, double height, double textHeight, int scale);

    }

    internal class AnnotationPosition : IAnnotationPosition {
        public bool NeedFlip { get; set; }

        public XYZ ViewRightDirection { get; set; }

        public FamilySymbol FamilySymbol { get; set; }

        public XYZ Point { get; set; }

        public XYZ GetPoint(XYZ min, XYZ max, double width, double height, double textHeight, int scale) {
            throw new System.NotImplementedException();
        }
    }
}
