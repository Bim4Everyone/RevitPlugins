using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitMarkAllDocuments.Services;

internal class DocumentService {
    public string GetDocumentFullName(Document document) {
        if(document.IsWorkshared) {
            var modelPath = document.GetWorksharingCentralModelPath();
            return ModelPathUtils.ConvertModelPathToUserVisiblePath(modelPath);
        } else {
            return document.Title;
        }
    }
}
