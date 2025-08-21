using System;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

using RevitSectionsConstructor.Models;

namespace RevitSectionsConstructor.Services;
internal class DocumentSaver {
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localization;

    public DocumentSaver(RevitRepository revitRepository, ILocalizationService localization) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
    }


    internal void SaveDocument(string path) {
        if(_revitRepository.Document.IsWorkshared) {
            SaveWorksharedDocument(path);
        } else {
            SaveNotWorksharedDocument(path);
        }
    }

    private void SaveWorksharedDocument(string path) {
        var wsOptions = new WorksharingSaveAsOptions() {
            SaveAsCentral = true
        };

        var options = new SaveAsOptions() {
            Compact = true,
            OverwriteExistingFile = true,
            MaximumBackups = 1
        };
        options.SetWorksharingOptions(wsOptions);

        _revitRepository.Document.SaveAs(path, options);

        var swcOptions = new SynchronizeWithCentralOptions() {
            Comment = _localization.GetLocalizedString("SyncTitle"),
            Compact = true
        };
        swcOptions.SetRelinquishOptions(new RelinquishOptions(true));

        _revitRepository.Document.SynchronizeWithCentral(new TransactWithCentralOptions(), swcOptions);
    }

    private void SaveNotWorksharedDocument(string path) {
        var options = new SaveAsOptions() {
            Compact = true,
            OverwriteExistingFile = true,
            MaximumBackups = 1
        };
        _revitRepository.Document.SaveAs(path, options);
    }
}
