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

namespace RevitClashDetective.Models.RevitClashReport {
    internal class ReportLoader {
        private readonly RevitRepository _revitRepository;

        public ReportLoader(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
        }

        public IEnumerable<ClashModel> GetClashes(string path) {
            if(string.IsNullOrEmpty(path)) {
                throw new ArgumentException($"'{nameof(path)}' cannot be null or empty.", nameof(path));
            }

            if(!File.Exists(path)) {
                throw new ArgumentException($"Путь \"{path}\" недоступен.", nameof(path));
            }

            if(!IsCorrectFileName(path)) {
                throw new ArgumentException($"Неверный файл отчета.", nameof(path));
            }

            return File.ReadAllLines(path).Skip(3)
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
        }

        private bool IsCorrectFileName(string path) {
            var fileString = File.ReadAllLines(path).Skip(1).FirstOrDefault();
            if(string.IsNullOrEmpty(fileString)) {
                return false;
            }
            var match = Regex.Match(fileString, @"</b>(?'fileName'.+?)<br>");
            if(!match.Success) {
                return false;
            }

            var fileName = Path.GetFileNameWithoutExtension(match.Groups["fileName"].Value.Trim());
            return _revitRepository.GetDocumentName().Equals(_revitRepository.GetDocumentName(fileName), StringComparison.CurrentCultureIgnoreCase);
        }

        private Element GetElement(IEnumerable<string> elementString) {
            return GetElement(GetId(elementString.LastOrDefault()), GetFile(elementString.FirstOrDefault().Trim()));
        }

        private int GetId(string idString) {
            var match = Regex.Match(idString, @"(?'id'\d+)");
            if(match.Success) {
                return int.Parse(match.Groups["id"].Value);
            }

            return -1;
        }

        private string GetFile(string fileString) {
            return fileString.EndsWith(".rvt", StringComparison.CurrentCultureIgnoreCase) ? fileString : null;
        }

        private Element GetElement(int id, string filename) {
            var elementId = new ElementId(id);

            if(filename == null && elementId.IsNotNull()) {
                return _revitRepository.GetElement(elementId);
            }

            var doc = _revitRepository.GetRevitLinkInstances()
                                      .FirstOrDefault(item => item.Name.StartsWith(filename, StringComparison.CurrentCultureIgnoreCase))
                                      ?.GetLinkDocument();

            return doc?.GetElement(elementId);
        }
    }
}