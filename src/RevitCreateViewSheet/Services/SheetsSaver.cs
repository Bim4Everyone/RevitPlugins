using System;
using System.Collections.Generic;
using System.Threading;

using dosymep.Revit;
using dosymep.SimpleServices;

using RevitCreateViewSheet.Models;

namespace RevitCreateViewSheet.Services {
    internal class SheetsSaver {
        private readonly RevitRepository _revitRepository;
        private readonly ILocalizationService _localizationService;

        public SheetsSaver(RevitRepository revitRepository, ILocalizationService localizationService) {
            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
            _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        }


        public void SaveSheets(
            ICollection<SheetModel> sheets,
            IProgress<int> progress = null,
            CancellationToken ct = default) {

            string title = _localizationService.GetLocalizedString("TODO");
            using(var transaction = _revitRepository.Document.StartTransaction(title)) {
                int i = 0;
                foreach(var sheet in sheets) {
                    ct.ThrowIfCancellationRequested();
                    sheet.SaveChanges(_revitRepository);
                    progress?.Report(++i);
                }
                transaction.Commit();
            }
        }
    }
}
