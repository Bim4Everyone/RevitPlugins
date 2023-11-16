using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Models.RevitClashReport {
    internal class NavisHtmlClashesLoader : BaseClashesLoader, IClashesLoader {
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
                throw new ArgumentException($"���� \"{FilePath}\" ����������.", nameof(FilePath));
            }

            var htmlText = string.Join("", File.ReadAllLines(FilePath));
            var index = htmlText.IndexOf("<table class=\"mainTable\">", StringComparison.CurrentCultureIgnoreCase);
            if(index < 0) {
                return null;
            }

            var rows = Regex.Matches(htmlText.Substring(index), @"<tr class=""contentRow"">(?'row'.+?)<\/tr>")
                            .Cast<Match>()
                            .Select(GetCells);
            if(!IsCorrectFileName(rows.FirstOrDefault())) {
                throw new ArgumentException($"������ ����� � ��������� ������ � ������ �����.");
            }
            return rows.Select(item => new {
                LeftElement = GetElement(item),
                RightElement = GetElement(item.Reverse())
            })
                        .Where(item => item.LeftElement != null && item.RightElement != null)
                        .Select(item => new ClashModel(_revitRepository, item.LeftElement, item.RightElement))
                        .ToArray();
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
            var idString = cells.FirstOrDefault(item => item.IndexOf("ID") > 0);
            return GetId(idString);
        }

        private string GetFileName(IEnumerable<string> cells) {
            var fileCell = cells.FirstOrDefault(item => _extensions.Any(e => item.IndexOf(e, StringComparison.CurrentCultureIgnoreCase) > 0));
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
            var text = File.ReadAllText(FilePath);
            return text.Contains("table.titleTable")
                && text.Contains("table.testSummaryTable")
                && text.Contains("table.mainTable");
        }

        private bool IsCorrectFileName(ICollection<string> cells) {
            if(cells == null) {
                return false;
            }
            return new[] {
                GetFileName(cells),
                GetFileName(cells.Reverse())
            }.Any(item => _revitRepository.DocInfos.Any(d => d.Name.Equals(RevitRepository.GetDocumentName(Path.GetFileNameWithoutExtension(item)), StringComparison.CurrentCultureIgnoreCase)));
        }
    }
}