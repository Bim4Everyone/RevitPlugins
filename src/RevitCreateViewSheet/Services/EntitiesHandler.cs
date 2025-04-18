using System;
using System.Text;
using System.Threading;

using dosymep.Revit;
using dosymep.SimpleServices;

using RevitCreateViewSheet.Models;

namespace RevitCreateViewSheet.Services {
    internal class EntitiesHandler {
        private readonly RevitRepository _revitRepository;
        private readonly EntitiesTracker _entitiesTracker;
        private readonly ILocalizationService _localizationService;

        public EntitiesHandler(
            RevitRepository revitRepository,
            EntitiesTracker entitiesTracker,
            ILocalizationService localizationService) {

            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
            _entitiesTracker = entitiesTracker ?? throw new ArgumentNullException(nameof(entitiesTracker));
            _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        }


        public string HandleTrackedEntities(
            IProgress<int> progress = null,
            CancellationToken ct = default) {

            string title = _localizationService.GetLocalizedString("UpdateSheetsTransaction");
            StringBuilder sb = new();
            using(var transaction = _revitRepository.Document.StartTransaction(title)) {
                int i = 0;
                foreach(var id in _entitiesTracker.GetRemovedEntities()) {
                    ct.ThrowIfCancellationRequested();
                    if(!_revitRepository.DeleteElement(id)) {
                        sb.AppendLine(string.Format(
                            _localizationService.GetLocalizedString("Errors.CannotDeleteElement"), id.GetIdValue()));
                    }
                    progress?.Report(++i);
                }
                foreach(var sheet in _entitiesTracker.AliveSheets) {
                    ct.ThrowIfCancellationRequested();
                    try {
                        sheet.Saver.Save(sheet);
                        sheet.SetContentLocations();
                    } catch(InvalidOperationException) {
                        sb.AppendLine(string.Format(
                            _localizationService.GetLocalizedString("Errors.CannotSaveSheet"), sheet.SheetNumber));
                    }
                    progress?.Report(++i);
                }
                foreach(var viewPort in _entitiesTracker.AliveViewPorts) {
                    ct.ThrowIfCancellationRequested();
                    try {
                        viewPort.Saver.Save(viewPort);
                    } catch(InvalidOperationException) {
                        sb.AppendLine(string.Format(
                            _localizationService.GetLocalizedString("Errors.CannotSaveViewPort"), viewPort.Name, viewPort.Sheet.SheetNumber));
                    }
                    progress?.Report(++i);
                }
                foreach(var schedule in _entitiesTracker.AliveSchedules) {
                    ct.ThrowIfCancellationRequested();
                    try {
                        schedule.Saver.Save(schedule);
                    } catch(InvalidOperationException) {
                        sb.AppendLine(string.Format(
                            _localizationService.GetLocalizedString("Errors.CannotSaveSchedule"),
                            schedule.Name, schedule.Sheet.SheetNumber));
                    }
                    progress?.Report(++i);
                }
                foreach(var annotation in _entitiesTracker.AliveAnnotations) {
                    ct.ThrowIfCancellationRequested();
                    try {
                        annotation.Saver.Save(annotation);
                    } catch(InvalidOperationException) {
                        sb.AppendLine(string.Format(
                            _localizationService.GetLocalizedString("Errors.CannotSaveAnnotation"),
                            annotation.SymbolName, annotation.Sheet.SheetNumber));
                    }
                    progress?.Report(++i);
                }
                transaction.Commit();
            }
            return sb.ToString();
        }
    }
}
