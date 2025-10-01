using System;
using System.Collections.Generic;
using System.IO;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Models.RevitClashReport;
internal class PluginClashesLoader : IClashesLoader {
    private readonly Document _document;

    public PluginClashesLoader(string path, Document document) {
        if(document is null) { throw new ArgumentNullException(nameof(document)); }

        FilePath = path;
        _document = document;
    }

    public string FilePath { get; }

    public IEnumerable<ReportModel> GetReports() {
        try {
            var configLoader = new ConfigLoader(_document);
            var clashes = configLoader.Load<ClashesConfig>(FilePath).Clashes;
            string name = Path.GetFileNameWithoutExtension(FilePath);
            return new ReportModel[] { new(name, clashes) };
        } catch(pyRevitLabs.Json.JsonSerializationException) {
            throw new ArgumentException("Неверный файл конфигурации.");
        }

    }

    public bool IsValid() {
        return FilePath.EndsWith(".json");
    }
}
