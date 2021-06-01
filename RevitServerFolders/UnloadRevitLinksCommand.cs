using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

namespace RevitServerFolders {
    internal class UnloadRevitLinksCommand {
        public Application Application { get; set; }

        public string SourceFolderName { get; set; }

        public void Execute() {
            if(Application == null) {
                throw new InvalidOperationException("Не было инициализировано свойство приложения Revit.");
            }

            if(string.IsNullOrEmpty(SourceFolderName)) {
                throw new InvalidOperationException("Перед использованием укажите папку с файлами Revit.");
            }

            foreach(string rvtFileName in Directory.EnumerateFiles(SourceFolderName, "*.rvt")) {
                ModelPath rvtModelPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(rvtFileName);

                TransmissionData transData = TransmissionData.ReadTransmissionData(rvtModelPath);
                transData.IsTransmitted = true;

                IEnumerable<ExternalFileReference> externalReferences = transData.GetAllExternalFileReferenceIds()
                    .Select(item => transData.GetLastSavedReferenceData(item))
                    .Where(item => item.ExternalFileReferenceType == ExternalFileReferenceType.RevitLink);

                foreach(ExternalFileReference externalReference in externalReferences) {
                    transData.SetDesiredReferenceData(externalReference.GetReferencingId(), externalReference.GetPath(), externalReference.PathType, false);
                }

                TransmissionData.WriteTransmissionData(rvtModelPath, transData);
            }
        }
    }
}
