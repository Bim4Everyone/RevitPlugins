using Autodesk.Revit.DB;

namespace RevitShakeSpecs.Models;
internal class SheetUtils {
    public SheetUtils(Document document, ViewSheet viewSheet) {
        ViewSheet = viewSheet;
        Doc = document;
    }

    public ViewSheet ViewSheet { get; set; }
    internal Document Doc { get; set; }


    public void FindAndShakeSpecsOnSheet() {
        var allSpecsViews = new FilteredElementCollector(Doc, ViewSheet.Id)
            .OfClass(typeof(ScheduleSheetInstance))
            .WhereElementIsNotElementType()
            .ToElements();

        if(allSpecsViews.Count == 0) {
            return;
        }

        foreach(var item in allSpecsViews) {
            if(item is not ScheduleSheetInstance spec) { continue; }

            var point_1 = spec.Point;
            spec.Point = new XYZ(0, 0, 0);
            Doc.Regenerate();

            spec.Point = point_1;
            Doc.Regenerate();
        }
    }

}
