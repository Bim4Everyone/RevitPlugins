using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitClashDetective.Models.Clashes {
    internal class ElementModel : IEquatable<ElementModel> {
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

        public Element GetElement() {
            var doc = _revitRepository.GetDocuments().FirstOrDefault(item => _revitRepository.GetDocumentName(item).Equals(DocumentName));
            var id = new ElementId(Id);
            if(doc != null && id.IsNotNull()) {
                return doc.GetElement(id);
            }
            return null;
        }

        public override bool Equals(object obj) {
            return Equals(obj as ElementModel);
        }

        public override int GetHashCode() {
            int hashCode = 426527819;
            hashCode = hashCode * -1521134295 + Id.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Category);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Level);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(DocumentName);
            return hashCode;
        }

        public bool Equals(ElementModel other) {
            return other != null
                && Id == other.Id
                && Name == other.Name
                && Category == other.Category
                && Level == other.Level
                && DocumentName == other.DocumentName;
        }
    }
}
