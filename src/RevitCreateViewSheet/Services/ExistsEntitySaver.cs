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
            if(!sheetModel.TryGetViewSheet(out var viewSheet)) {
                throw new InvalidOperationException();
            }
            _revitRepository.UpdateViewSheet(viewSheet, sheetModel);
        }

        public void Save(ViewPortModel viewModel) {
            if(!viewModel.TryGetViewport(out var viewPort)) {
                throw new InvalidOperationException();
            }
            if(viewModel.ViewPortType is null) {
                throw new InvalidOperationException(
                    $"Перед сохранением измененного видового экрана необходимо назначить {nameof(viewModel.ViewPortType)}");
            }
            _revitRepository.UpdateViewPort(viewPort, viewModel.ViewPortType.Id);
        }

        public void Save(ScheduleModel scheduleModel) {
            // nothing 
        }

        public void Save(AnnotationModel annotationModel) {
            // nothing
        }
    }
}
