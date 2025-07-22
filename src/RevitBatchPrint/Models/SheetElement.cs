using Autodesk.Revit.DB;

namespace RevitBatchPrint.Models;

internal class SheetElement {
    public ViewSheet ViewSheet { get; set; }
    public PrintSheetSettings PrintSheetSettings { get; set; }
}
