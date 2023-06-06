using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

using Autodesk.Revit.DB;

using RevitShakeSpecs.ViewModels;

namespace RevitShakeSpecs.Models {
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
                ScheduleSheetInstance spec = item as ScheduleSheetInstance;
                if(spec == null) { continue; }

                XYZ point_1 = spec.Point;
                spec.Point = new XYZ(0, 0, 0);
                Doc.Regenerate();

                spec.Point = point_1;
                Doc.Regenerate();
            }
        }

    }
}
