using System;

using dosymep.SimpleServices;

using RevitCreateViewSheet.Models;

namespace RevitCreateViewSheet.Services {
    internal class ExistsEntitySaver : IEntitySaver {
        private readonly RevitRepository _revitRepository;
        private readonly ILocalizationService _localizationService;

        public ExistsEntitySaver(RevitRepository revitRepository, ILocalizationService localizationService) {
            _revitRepository = revitRepository
                ?? throw new ArgumentNullException(nameof(revitRepository));
            _localizationService = localizationService
                ?? throw new ArgumentNullException(nameof(localizationService));
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
                    string.Format(
                        _localizationService.GetLocalizedString("Errors.ViewPortModel.PropertyNotSet",
                        nameof(viewPortModel.ViewPortType))));
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
