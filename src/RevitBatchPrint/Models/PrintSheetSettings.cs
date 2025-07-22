using Autodesk.Revit.DB;

namespace RevitBatchPrint.Models {
    internal class PrintSheetSettings {
        public SheetFormat SheetFormat { get; set; }
        public PageOrientationType FormatOrientation { get; set; }
    }
}
