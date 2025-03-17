using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Models.RevitClashReport {
    internal class NavisXmlClashesLoader : BaseClashesLoader, IClashesLoader {
        private readonly RevitRepository _revitRepository;

        public NavisXmlClashesLoader(RevitRepository revitRepository, string path) {
            if(string.IsNullOrWhiteSpace(path)) {
                throw new ArgumentException(nameof(path));
            }
            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
            FilePath = path;
        }

        public string FilePath { get; }

        public IEnumerable<ReportModel> GetReports() {
            if(string.IsNullOrWhiteSpace(FilePath)) {
                throw new ArgumentException(nameof(FilePath));
            }

            if(!File.Exists(FilePath)) {
                throw new ArgumentException($"Файл \"{FilePath}\" отсутствует.", nameof(FilePath));
            }

            var xmlDoc = XDocument.Load(FilePath);
            if(!IsCorrectFileName(xmlDoc)) {
                throw new ArgumentException("Попытка загрузки файла отчета, созданного в другом проекте.");
            }

            return xmlDoc.Descendants("clashtest")
                .Select(ct => new ReportModel((string) ct.Attribute("name"), GetClashModels(ct)));
        }

        public bool IsValid() {
            return !string.IsNullOrEmpty(FilePath)
                && FilePath.EndsWith(".xml");
        }


        private ICollection<ClashModel> GetClashModels(XElement clashTest) {
            return clashTest.Descendants("clashresult")
                .Select(c => GetClashModel(c))
                .Where(c => c.MainElement.Id.IsNotNull() && c.OtherElement.Id.IsNotNull())
                .ToArray();
        }

        private ClashModel GetClashModel(XElement clashResult) {
            var elements = clashResult.Descendants("clashobject")
                .Take(2)
                .Select(c => GetElementModel(c))
                .ToArray();
            var clash = new ClashModel();
            if(elements.Length == 2) {
                clash.MainElement = elements[0];
                clash.OtherElement = elements[1];
            }
            return clash.SetRevitRepository(_revitRepository);
        }

        private ElementModel GetElementModel(XElement clashObject) {
            var tags = clashObject.Descendants("smarttag").ToDictionary(
                t => t.Element("name").Value.ToLower(),
                t => t.Element("value").Value);
            string file = tags.TryGetValue("элемент файл источника", out string fileName) ? fileName : null;
            ElementId id = tags.TryGetValue("объект id", out string idStr) ? GetId(idStr) : ElementId.InvalidElementId;
            return GetElementModel(file, id);
        }

        private ElementModel GetElementModel(string fileName, ElementId id) {
            var el = _revitRepository.GetElement(fileName, id);
            if(el != null) {
                ElementModel elModel = new ElementModel(el);
                var docInfo = elModel.GetDocInfo(_revitRepository.DocInfos);
                elModel.TransformModel = new TransformModel(docInfo.Transform);
                return elModel;
            } else {
                return new ElementModel() { Id = ElementId.InvalidElementId };
            }
        }

        private bool IsCorrectFileName(XDocument xDoc) {
            if(xDoc is null) {
                return false;
            }
            var file = xDoc.Descendants("smarttag")
                .FirstOrDefault(tag => tag.Element("value")?.Value?.EndsWith(".rvt") ?? false)?.Element("value").Value
                ?? string.Empty;
            return _revitRepository.DocInfos.Any(d => d.Name.Equals(
                RevitRepository.GetDocumentName(file),
                StringComparison.CurrentCultureIgnoreCase));
        }
    }
}
