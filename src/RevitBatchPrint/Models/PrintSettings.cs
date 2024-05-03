using Autodesk.Revit.DB;

namespace RevitBatchPrint.Models {
    internal class PrintSettings {
        public Format Format { get; set; }
        public PageOrientationType FormatOrientation { get; set; }
    }
}