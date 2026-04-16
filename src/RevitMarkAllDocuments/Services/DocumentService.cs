using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitMarkAllDocuments.Services;

internal class DocumentService {
    public string GetDocumentFullName(Document document) {
        if(document.IsWorkshared) {
            var modelPath = document.GetWorksharingCentralModelPath();
            string path = ModelPathUtils.ConvertModelPathToUserVisiblePath(modelPath);

            return ExtractFileName(path);
        } else {
            return Path.GetFileNameWithoutExtension(document.Title);
        }
    }

    private string ExtractFileName(string path) {
        if(string.IsNullOrEmpty(path))
            return string.Empty;

        path = path.Replace('\\', '/');

        string lastSegment = path.Split('/').LastOrDefault();

        return Path.GetFileNameWithoutExtension(lastSegment);
    }
}
