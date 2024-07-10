using System;

using Autodesk.Revit.DB;

using RevitSectionsConstructor.Models;

namespace RevitSectionsConstructor.Services {
    internal class DocumentSaver {
        private readonly RevitRepository _revitRepository;

        public DocumentSaver(RevitRepository revitRepository) {
            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        }


        internal void SaveDocument(string path) {
            if(_revitRepository.Document.IsWorkshared) {
                SaveWorksharedDocument(path);
            } else {
                SaveNotWorksharedDocument(path);
            }
        }

        private void SaveWorksharedDocument(string path) {
            WorksharingSaveAsOptions wsOptions = new WorksharingSaveAsOptions() {
                SaveAsCentral = true
            };

            SaveAsOptions options = new SaveAsOptions() {
                Compact = true,
                OverwriteExistingFile = true,
                MaximumBackups = 1
            };
            options.SetWorksharingOptions(wsOptions);

            _revitRepository.Document.SaveAs(path, options);

            SynchronizeWithCentralOptions swcOptions = new SynchronizeWithCentralOptions() {
                Comment = "Синхронизация после обработки групп",
                Compact = true
            };
            swcOptions.SetRelinquishOptions(new RelinquishOptions(true));

            _revitRepository.Document.SynchronizeWithCentral(new TransactWithCentralOptions(), swcOptions);
        }

        private void SaveNotWorksharedDocument(string path) {
            SaveAsOptions options = new SaveAsOptions() {
                Compact = true,
                OverwriteExistingFile = true,
                MaximumBackups = 1
            };
            _revitRepository.Document.SaveAs(path, options);
        }
    }
}
