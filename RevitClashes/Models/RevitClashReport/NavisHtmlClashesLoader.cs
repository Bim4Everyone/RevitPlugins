using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Models.RevitClashReport {
    internal class NavisHtmlClashesLoader : IClashesLoader {
        private readonly RevitRepository _revitRepository;
        private readonly string[] _extensions = new[] { ".nwc", ".nwf", ".nwd" };

        public NavisHtmlClashesLoader(RevitRepository revitRepository, string path) {
            this._revitRepository = revitRepository;
            FilePath = path;
        }
        public string FilePath { get; }

        public IEnumerable<ClashModel> GetClashes() {
            if(string.IsNullOrEmpty(FilePath)) {
                throw new ArgumentException($"'{nameof(FilePath)}' cannot be null or empty.", nameof(FilePath));
            }

            if(!File.Exists(FilePath)) {
                throw new ArgumentException($"Путь \"{FilePath}\" недоступен.", nameof(FilePath));
            }

            var htmlText = string.Join("", File.ReadAllLines(FilePath));
            var index = htmlText.IndexOf("<table class=\"mainTable\">", StringComparison.CurrentCultureIgnoreCase);
            if(index < 0) {
                return null;
            }

            return Regex.Matches(htmlText.Substring(index), @"<tr class=""contentRow"">(?'row'.+?)<\/tr>")
                            .Cast<Match>()
                            .Select(GetCells)
                            .Select(item => new {
                                LeftElement = GetElement(item),
                                RightElement = GetElement(item.Reverse())
                            })
                            .Where(item => item.LeftElement != null && item.RightElement != null)
                            .Select(item => new ClashModel(_revitRepository, item.LeftElement, item.RightElement))
                            .ToArray();
        }

        private Element GetElement(IEnumerable<string> cells) {
            return _revitRepository.GetElement(GetFileName(cells), GetId(cells));
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

        private int GetId(IEnumerable<string> cells) {
            var idString = cells.FirstOrDefault(item => item.IndexOf("ID") > 0);
            if(idString == null) {
                return -1;
            }
            var match = Regex.Match(idString, @"(?'id'\d+)");
            if(match.Success) {
                return int.Parse(match.Groups["id"].Value);
            }

            return -1;
        }

        private string GetFileName(IEnumerable<string> cells) {
            var fileCell = cells.FirstOrDefault(item => _extensions.Any(e => item.IndexOf(e, StringComparison.CurrentCultureIgnoreCase) > 0));
            var fileCellInfo = new {
                Extension = _extensions.FirstOrDefault(e => fileCell.IndexOf(e, StringComparison.CurrentCultureIgnoreCase) > 0),
                CellContent = fileCell
            };
            if(fileCellInfo == null) {
                return null;
            }
            var match = Regex.Match(fileCellInfo.CellContent, $"(?'fileName'[^>\\s]+?){fileCellInfo.Extension}");
            return match.Success ? match.Groups["fileName"].Value : null;
        }

        private bool TextContainsTables() {
            var text = File.ReadAllText(FilePath);
            return text.Contains("table.titleTable")
                && text.Contains("table.testSummaryTable")
                && text.Contains("table.mainTable");
        }
    }
}