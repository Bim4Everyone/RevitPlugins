using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Models.RevitClashReport;
internal class NavisHtmlClashesLoader : BaseClashesLoader, IClashesLoader {
    private readonly RevitRepository _revitRepository;
    private readonly string[] _extensions = new[] { ".nwc", ".nwf", ".nwd" };

    public NavisHtmlClashesLoader(RevitRepository revitRepository, string path) {
        _revitRepository = revitRepository;
        FilePath = path;
    }
    public string FilePath { get; }

    public IEnumerable<ReportModel> GetReports() {
        if(string.IsNullOrEmpty(FilePath)) {
            throw new ArgumentException($"'{nameof(FilePath)}' cannot be null or empty.", nameof(FilePath));
        }

        if(!File.Exists(FilePath)) {
            throw new ArgumentException($"Файл \"{FilePath}\" отсутствует.", nameof(FilePath));
        }

        string htmlText = string.Join("", File.ReadAllLines(FilePath));
        int index = htmlText.IndexOf("<table class=\"mainTable\">", StringComparison.CurrentCultureIgnoreCase);
        if(index < 0) {
            return null;
        }

        var rows = Regex.Matches(htmlText.Substring(index), @"<tr class=""contentRow"">(?'row'.+?)<\/tr>")
                        .Cast<Match>()
                        .Select(GetCells);
        if(!IsCorrectFileName(rows.FirstOrDefault())) {
            throw new ArgumentException("Нельзя загрузить проверки, сделанные в другом проекте.");
        }
        var clashes = rows.Select(item => new {
            LeftElement = GetElement(item),
            RightElement = GetElement(item.Reverse())
        })
                    .Where(item => item.LeftElement != null && item.RightElement != null)
                    .Select(item => new ClashModel(_revitRepository, item.LeftElement, item.RightElement))
                    .ToArray();
        return new ReportModel[] { new(Path.GetFileNameWithoutExtension(FilePath), clashes) };
    }


    public bool IsValid() {
        return !string.IsNullOrEmpty(FilePath)
               && FilePath.EndsWith(".html")
               && TextContainsTables();
    }

    private static string[] GetCells(Match item) {
        return Regex.Matches(item.Groups["row"].Value, @"<td class="".+?"">(?'cell'.+?)<\/td>")
                    .Cast<Match>()
                    .Select(i => i.Groups["cell"].Value)
                    .ToArray();
    }

    private Element GetElement(IEnumerable<string> cells) {
        return _revitRepository.GetElement(GetFileName(cells), GetId(cells));
    }

    private ElementId GetId(IEnumerable<string> cells) {
        string idString = cells.FirstOrDefault(item => item.IndexOf("ID") > 0);
        return GetId(idString);
    }

    private string GetFileName(IEnumerable<string> cells) {
        string fileCell = cells.FirstOrDefault(item => _extensions.Any(e => item.IndexOf(e, StringComparison.CurrentCultureIgnoreCase) > 0));
        if(fileCell == null) {
            return null;
        }
        var fileCellInfo = new {
            Extension = _extensions.FirstOrDefault(e => fileCell.IndexOf(e, StringComparison.CurrentCultureIgnoreCase) > 0),
            CellContent = fileCell
        };

        var match = Regex.Match(fileCellInfo.CellContent, $"(?'fileName'[^>&gt;]+?){fileCellInfo.Extension}");
        return match.Success ? match.Groups["fileName"].Value.Trim() : null;
    }

    private bool TextContainsTables() {
        string text = File.ReadAllText(FilePath);
        return text.Contains("table.titleTable")
            && text.Contains("table.testSummaryTable")
            && text.Contains("table.mainTable");
    }

    private bool IsCorrectFileName(ICollection<string> cells) {
        return cells != null && new[] {
            GetFileName(cells),
            GetFileName(cells.Reverse())
        }.Any(item => _revitRepository.DocInfos.Any(d => d.Name.Equals(RevitRepository.GetDocumentName(Path.GetFileNameWithoutExtension(item)), StringComparison.CurrentCultureIgnoreCase)));
    }
}
