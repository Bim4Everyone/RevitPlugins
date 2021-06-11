using Autodesk.Revit.DB;

namespace RevitBatchPrint {
    internal class PrintSettings {
        public Format Format { get; set; }
        public PageOrientationType FormatOrientation { get; set; }
    }
}
