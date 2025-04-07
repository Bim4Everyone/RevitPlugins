using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitCreateViewSheet.Models;

namespace RevitCreateViewSheet.Services {
    internal interface ISheetItemsFactory {
        ViewPortModel CreateViewPort(SheetModel sheetModel, ICollection<View> disabledViews);

        ScheduleModel CreateSchedule(SheetModel sheetModel);

        AnnotationModel CreateAnnotation(SheetModel sheetModel);
    }
}
