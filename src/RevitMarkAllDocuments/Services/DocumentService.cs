using System;
using System.IO;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitMarkAllDocuments.Services;

internal class DocumentService : IDocumentInterface {
    private static readonly string[] _endings = ["_отсоединено", "_detached"];

    public string GetDocumentFullName(Document document) {
        if(document.IsWorkshared) {
            var modelPath = document.GetWorksharingCentralModelPath();
            if(modelPath != null) {
                string path = ModelPathUtils.ConvertModelPathToUserVisiblePath(modelPath);
                string name = ExtractFileName(path);
                if(!string.IsNullOrEmpty(name)) {
                    return name;
                }
            }
        }

        return GetTitleWithoutSuffixes(document);
    }

    /// <summary>
    /// Возвращает имя документа без суффикса пользователя и суффикса "отсоединено"
    /// </summary>
    private string GetTitleWithoutSuffixes(Document document) {
        string name = Path.GetFileNameWithoutExtension(document.Title);
        string[] endings = [.. _endings, "_" + document.Application.Username];

        foreach(string ending in endings) {
            int index = name.IndexOf(ending, StringComparison.OrdinalIgnoreCase);
            if(index > -1) {
                name = name.Substring(0, index);
            }
        }

        return name;
    }

    private string ExtractFileName(string path) {
        if(string.IsNullOrEmpty(path)) {
            return string.Empty;
        }

        path = path.Replace('\\', '/');

        string lastSegment = path.Split('/').LastOrDefault();

        return Path.GetFileNameWithoutExtension(lastSegment);
    }
}
