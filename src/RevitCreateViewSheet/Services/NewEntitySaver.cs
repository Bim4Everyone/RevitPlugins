using System;

using dosymep.SimpleServices;

using RevitCreateViewSheet.Models;

namespace RevitCreateViewSheet.Services {
    internal class NewEntitySaver : IEntitySaver {
        private readonly RevitRepository _revitRepository;
        private readonly ILocalizationService _localizationService;

        public NewEntitySaver(RevitRepository revitRepository, ILocalizationService localizationService) {
            _revitRepository = revitRepository
                ?? throw new ArgumentNullException(nameof(revitRepository));
            _localizationService = localizationService
                ?? throw new ArgumentNullException(nameof(localizationService));
        }

        public void Save(SheetModel sheetModel) {
            if(sheetModel.TitleBlockSymbol is null) {
                throw new InvalidOperationException(
                    _localizationService.GetLocalizedString("Errors.SheetModel.TitleBlockNotSet"));
            }
            if(string.IsNullOrWhiteSpace(sheetModel.SheetNumber)) {
                throw new InvalidOperationException(
                    _localizationService.GetLocalizedString("Errors.SheetModel.SheetNumberNotSet"));
            }
            _revitRepository.CreateSheet(sheetModel);
        }

        public void Save(ViewPortModel viewPortModel) {
            if(!viewPortModel.Sheet.TryGetViewSheet(out _)) {
                throw new InvalidOperationException(
                    _localizationService.GetLocalizedString("Errors.CannotCreateViewPortOnNotCreatedSheet"));
            }
            if(viewPortModel.Location is null) {
                throw new InvalidOperationException(
                    _localizationService.GetLocalizedString(
                        string.Format(
                            _localizationService.GetLocalizedString("Errors.ViewPortModel.PropertyNotSet"),
                            nameof(viewPortModel.Location))));
            }
            if(viewPortModel.ViewPortType is null) {
                throw new InvalidOperationException(
                    _localizationService.GetLocalizedString(
                        string.Format(
                            _localizationService.GetLocalizedString("Errors.ViewPortModel.PropertyNotSet"),
                            nameof(viewPortModel.ViewPortType))));
            }
            _revitRepository.CreateViewPort(viewPortModel);
        }

        public void Save(ScheduleModel scheduleModel) {
            if(!scheduleModel.Sheet.TryGetViewSheet(out var viewSheet)) {
                throw new InvalidOperationException(
                    _localizationService.GetLocalizedString("Errors.CannotCreateScheduleOnNotCreatedSheet"));
            }
            if(scheduleModel.Location is null) {
                throw new InvalidOperationException(
                    string.Format(
                        _localizationService.GetLocalizedString("Errors.ScheduleModel.PropertyNotSet"),
                        nameof(scheduleModel.Location)));
            }
#if REVIT_2022_OR_GREATER
            _revitRepository.CreateSchedule(viewSheet.Id, scheduleModel.ViewSchedule.Id, scheduleModel.Location, scheduleModel.SegmentIndex);
#else
            _revitRepository.CreateSchedule(viewSheet.Id, scheduleModel.ViewSchedule.Id, scheduleModel.Location);
#endif
        }

        public void Save(AnnotationModel annotationModel) {
            if(!annotationModel.Sheet.TryGetViewSheet(out var viewSheet)) {
                throw new InvalidOperationException(
                    _localizationService.GetLocalizedString("Errors.CannotCreateAnnotationOnNotCreatedSheet"));
            }
            if(annotationModel.Location is null) {
                throw new InvalidOperationException(
                    string.Format(
                        _localizationService.GetLocalizedString("Errors.AnnotationModel.PropertyNotSet"),
                        nameof(annotationModel.Location)));
            }
            _revitRepository.CreateAnnotation(viewSheet, annotationModel.AnnotationSymbolType, annotationModel.Location);
        }
    }
}
