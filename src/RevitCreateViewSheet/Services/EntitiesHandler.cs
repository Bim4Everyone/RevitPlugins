using System;
using System.Threading;

using dosymep.SimpleServices;

using RevitCreateViewSheet.Models;

namespace RevitCreateViewSheet.Services {
    internal class EntitiesHandler {
        private readonly RevitRepository _revitRepository;
        private readonly ILocalizationService _localizationService;

        public EntitiesHandler(RevitRepository revitRepository, ILocalizationService localizationService) {
            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
            _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        }


        public void HandleEntities(
            IProgress<int> progress = null,
            CancellationToken ct = default) {
            throw new NotImplementedException();
            //string title = _localizationService.GetLocalizedString("TODO");
            //using(var transaction = _revitRepository.Document.StartTransaction(title)) {
            //    int i = 0;
            //    foreach(var sheet in []) {
            //        ct.ThrowIfCancellationRequested();
            //        sheet.SaveChanges(_revitRepository);
            //        progress?.Report(++i);
            //    }
            //    transaction.Commit();
            //}
        }
    }
}
