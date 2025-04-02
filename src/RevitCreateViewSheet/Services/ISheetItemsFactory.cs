using RevitCreateViewSheet.Models;

namespace RevitCreateViewSheet.Services {
    internal interface ISheetItemsFactory {
        ViewPortModel CreateViewPort(SheetModel sheetModel);

        ScheduleModel CreateSchedule(SheetModel sheetModel);

        AnnotationModel CreateAnnotation(SheetModel sheetModel);
    }
}
