using System;

using RevitCreateViewSheet.Models;

namespace RevitCreateViewSheet.Services {
    internal class NewEntitySaver : IEntitySaver {
        private readonly RevitRepository _revitRepository;
        // TODO локализация
        public NewEntitySaver(RevitRepository revitRepository) {
            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        }

        public void Save(SheetModel sheetModel) {
            if(sheetModel.TitleBlockSymbol is null) {
                throw new InvalidOperationException($"У листа не задана основная надпись");
            }
            if(string.IsNullOrWhiteSpace(sheetModel.SheetNumber)) {
                throw new InvalidOperationException($"У листа не задан номер");
            }
            _revitRepository.CreateSheet(sheetModel);
        }

        public void Save(ViewPortModel viewPortModel) {
            if(!viewPortModel.Sheet.TryGetViewSheet(out _)) {
                throw new InvalidOperationException($"Нельзя разместить видовой экран на еще не созданный лист");
            }
            if(viewPortModel.Location is null) {
                throw new InvalidOperationException(
                    $"Перед сохранением нового видового экрана необходимо назначить {nameof(viewPortModel.Location)}");
            }
            if(viewPortModel.ViewPortType is null) {
                throw new InvalidOperationException(
                    $"Перед сохранением нового видового экрана необходимо назначить {nameof(viewPortModel.ViewPortType)}");
            }
            _revitRepository.CreateViewPort(viewPortModel);
        }

        public void Save(ScheduleModel scheduleModel) {
            if(!scheduleModel.Sheet.TryGetViewSheet(out var viewSheet)) {
                throw new InvalidOperationException($"Нельзя разместить спецификацию на еще не созданный лист");
            }
            if(scheduleModel.Location is null) {
                throw new InvalidOperationException(
                    $"Перед сохранением новой спецификации необходимо назначить {nameof(scheduleModel.Location)}");
            }
            _revitRepository.CreateSchedule(viewSheet.Id, scheduleModel.ViewSchedule.Id, scheduleModel.Location);
        }

        public void Save(AnnotationModel annotationModel) {
            if(!annotationModel.Sheet.TryGetViewSheet(out var viewSheet)) {
                throw new InvalidOperationException($"Нельзя разместить спецификацию на еще не созданный лист");
            }
            if(annotationModel.Location is null) {
                throw new InvalidOperationException(
                    $"Перед сохранением новой аннотации необходимо назначить {nameof(annotationModel.Location)}");
            }
            _revitRepository.CreateAnnotation(viewSheet, annotationModel.AnnotationSymbolType, annotationModel.Location);
        }
    }
}
