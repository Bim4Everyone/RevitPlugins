using System;
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


        public void HandleTrackedEntities(
            IProgress<int> progress = null,
            CancellationToken ct = default) {

            string title = _localizationService.GetLocalizedString("TODO");
            using(var transaction = _revitRepository.Document.StartTransaction(title)) {
                int i = 0;
                foreach(var id in _entitiesTracker.GetRemovedEntities()) {
                    ct.ThrowIfCancellationRequested();
                    _revitRepository.DeleteElement(id);
                    progress?.Report(++i);
                }
                foreach(var sheet in _entitiesTracker.AliveSheets) {
                    ct.ThrowIfCancellationRequested();
                    sheet.Saver.Save(sheet);
                    sheet.SetContentLocations();
                    progress?.Report(++i);
                }
                foreach(var viewPort in _entitiesTracker.AliveViewPorts) {
                    ct.ThrowIfCancellationRequested();
                    viewPort.Saver.Save(viewPort);
                    progress?.Report(++i);
                }
                foreach(var schedule in _entitiesTracker.AliveSchedules) {
                    ct.ThrowIfCancellationRequested();
                    schedule.Saver.Save(schedule);
                    progress?.Report(++i);
                }
                foreach(var annotation in _entitiesTracker.AliveAnnotations) {
                    ct.ThrowIfCancellationRequested();
                    annotation.Saver.Save(annotation);
                    progress?.Report(++i);
                }
                transaction.Commit();
            }
        }
    }
}
