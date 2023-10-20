using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitClashDetective.Models.Clashes {
    internal class ElementModel : IEquatable<ElementModel> {
        private readonly RevitRepository _revitRepository;

        public ElementModel(RevitRepository revitRepository, Element element) {
            if(element is null) { throw new ArgumentNullException(nameof(element)); }
            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));

            Id = element.Id;
            Name = element.Name;
            DocumentName = _revitRepository.GetDocumentName(element.Document);
            Category = element.Category?.Name;
            Level = revitRepository.GetLevelName(element);
        }

        public ElementModel() {

        }

        public ElementId Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Level { get; set; }
        public string DocumentName { get; set; }


        public Element GetElement(IEnumerable<DocInfo> docInfos) {
            var doc = docInfos.FirstOrDefault(item => item.Name.Equals(DocumentName));
            if(doc != null && Id.IsNotNull()) {
                return doc.Doc.GetElement(Id);
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
