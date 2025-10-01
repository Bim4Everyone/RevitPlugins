using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Models.RevitClashReport;
internal class ReportLoader {
    public static IEnumerable<ReportModel> GetReports(RevitRepository revitRepository, string path) {
        revitRepository.InitializeDocInfos();

        var loaders = new List<IClashesLoader>() {
            new PluginClashesLoader(path, revitRepository.Doc),
            new NavisXmlClashesLoader(revitRepository, path)
        };

        var loader = loaders.FirstOrDefault(item => item.IsValid());
        if(loader == null) {
            throw new ArgumentException("Неверный формат файла.");
        }

        var docNames = revitRepository.DocInfos.Select(item => RevitRepository.GetDocumentName(item.Doc)).ToList();

        var reports = loader.GetReports();
        foreach(var report in reports) {
            var filteredClashes = report.Clashes
                .Select(item => item.SetRevitRepository(revitRepository))
                .Where(item => item.IsValid(docNames));
            report.Clashes = new ReadOnlyCollection<ClashModel>(filteredClashes.ToArray());
        }
        return reports;
    }
}
