using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitClashDetective.Models.Clashes {
    internal class ElementModel {
        private readonly RevitRepository _revitRepository;

        public ElementModel(RevitRepository revitRepository, Element element) {
            _revitRepository = revitRepository;

            Id = element.Id.IntegerValue;
            Name = element.Name;
            DocumentName = _revitRepository.GetDocumentName(element.Document);
            Category = element.Category.Name;
            Level = GetLevel(element);
        }

        public ElementModel() {

        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Level { get; set; }
        public string DocumentName { get; set; }

        private string GetLevel(Element element) {
            string level;
            foreach(var paramName in RevitRepository.BaseLevelParameters) {
                level = element.IsExistsParam(paramName) ? element.GetParam(paramName).AsValueString() : null;
                if(level != null) {
                    return level;
                }
            }
            return element.LevelId == null ? null : element.Document.GetElement(element.LevelId)?.Name;
        }
    }
}
