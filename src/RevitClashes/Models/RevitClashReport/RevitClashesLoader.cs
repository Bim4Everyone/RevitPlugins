using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Models.RevitClashReport;
internal class RevitClashesLoader : BaseClashesLoader, IClashesLoader {
    private readonly RevitRepository _revitRepository;

    public string FilePath { get; }

    public RevitClashesLoader(RevitRepository revitRepository, string path) {
        _revitRepository = revitRepository;
        FilePath = path;
    }

    public IEnumerable<ReportModel> GetReports() {
        if(string.IsNullOrEmpty(FilePath)) {
            throw new ArgumentException($"'{nameof(FilePath)}' cannot be null or empty.", nameof(FilePath));
        }

        if(!File.Exists(FilePath)) {
            throw new ArgumentException($"Файл \"{FilePath}\" Отсутствует.", nameof(FilePath));
        }

        if(!IsCorrectFileName(FilePath)) {
            throw new ArgumentException("Нельзя загрузить проверки, сделанные в другом проекте..");
        }

        var clashes = File.ReadAllLines(FilePath).Skip(3)
                                      .Select(item => Regex.Matches(item, @"<td>(?'value'.+?)</td>")
                                                                       .Cast<Match>()
                                                                       .Select(i => i.Groups["value"].Value.Split(':'))
                                                                       .Skip(1))
                                      .Where(item => item.Any())
                                      .Select(item => new {
                                          LeftElement = GetElement(item.FirstOrDefault()),
                                          RightElement = GetElement(item.LastOrDefault())
                                      })
                                      .Where(item => item.LeftElement != null && item.RightElement != null)
                                      .Select(item => new ClashModel(_revitRepository, item.LeftElement, item.RightElement))
                                      .ToArray();
        return new ReportModel[] { new(Path.GetFileNameWithoutExtension(FilePath), clashes) };
    }

    public bool IsValid() {
        return FilePath.EndsWith(".html")
               && File.ReadAllLines(FilePath).Skip(2)
                                            .FirstOrDefault()
                                            ?.Equals("<p><table border=on>  <tr>  <td></td>  <td ALIGN=\"center\">A</td>  " +
                                            "<td ALIGN=\"center\">B</td>  </tr>", StringComparison.CurrentCultureIgnoreCase) == true;
    }

    private bool IsCorrectFileName(string path) {
        string fileString = File.ReadAllLines(path).Skip(1).FirstOrDefault();
        if(string.IsNullOrEmpty(fileString)) {
            return false;
        }
        var match = Regex.Match(fileString, @"</b>(?'fileName'.+?)<br>");
        if(!match.Success) {
            return false;
        }

        string fileName = Path.GetFileNameWithoutExtension(match.Groups["fileName"].Value.Trim());
        return _revitRepository.GetDocumentName().Equals(RevitRepository.GetDocumentName(fileName), StringComparison.CurrentCultureIgnoreCase);
    }


    private Element GetElement(IEnumerable<string> elementString) {
        return _revitRepository.GetElement(GetFile(elementString.FirstOrDefault()?.Trim()), GetId(elementString.LastOrDefault()));
    }

    private string GetFile(string fileString) {
        return fileString.EndsWith(".rvt", StringComparison.CurrentCultureIgnoreCase) ? Path.GetFileNameWithoutExtension(fileString) : null;
    }
}
