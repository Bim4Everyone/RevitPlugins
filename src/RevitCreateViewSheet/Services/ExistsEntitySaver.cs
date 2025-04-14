using System;

using RevitCreateViewSheet.Models;

namespace RevitCreateViewSheet.Services {
    internal class ExistsEntitySaver : IEntitySaver {
        private readonly RevitRepository _revitRepository;
        // TOTO localization
        public ExistsEntitySaver(RevitRepository revitRepository) {
            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        }


        public void Save(SheetModel sheetModel) {
            if(!sheetModel.TryGetViewSheet(out _)) {
                throw new InvalidOperationException();
            }
            _revitRepository.UpdateViewSheet(sheetModel);
        }

        public void Save(ViewPortModel viewPortModel) {
            if(!viewPortModel.TryGetViewport(out _)) {
                throw new InvalidOperationException();
            }
            if(viewPortModel.ViewPortType is null) {
                throw new InvalidOperationException(
                    $"Перед сохранением измененного видового экрана необходимо назначить " +
                    $"{nameof(viewPortModel.ViewPortType)}");
            }
            _revitRepository.UpdateViewPort(viewPortModel);
        }

        public void Save(ScheduleModel scheduleModel) {
            // nothing 
        }

        public void Save(AnnotationModel annotationModel) {
            // nothing
        }
    }
}
