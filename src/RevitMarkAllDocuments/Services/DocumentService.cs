using System;
using System.IO;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitMarkAllDocuments.Services;

internal class DocumentService : IDocumentInterface {
    private static readonly string[] _endings = ["_отсоединено", "_detached"];

    public string GetDocumentFullName(Document document) {
        string name = document.Title;
        string[] endings = [.. _endings, "_" + document.Application.Username];

        foreach(string ending in endings) {
            int index = name.IndexOf(ending, StringComparison.OrdinalIgnoreCase);
            if(index > -1) {
                name = name.Substring(0, index);
            }
        }

        return name;
    }
}
