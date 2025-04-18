using RevitCreateViewSheet.Models;

namespace RevitCreateViewSheet.Services {
    internal interface IEntitySaver {
        void Save(SheetModel sheetModel);

        void Save(ViewPortModel viewModel);

        void Save(ScheduleModel scheduleModel);

        void Save(AnnotationModel annotationModel);
    }
}
