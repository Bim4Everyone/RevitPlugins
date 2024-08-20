using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using pyRevitLabs.Json;

using RevitClashDetective.Models.Extensions;

namespace RevitClashDetective.Models.Clashes {
    internal class ElementModel : IEquatable<ElementModel> {
        [JsonConstructor]
        public ElementModel() { }

        public ElementModel(Element element)
            : this(element, Transform.Identity) {
        }

        /// <summary>
        /// Конструктор для элемента из связанного файла, который подгружен в активный документ с дублированием
        /// </summary>
        /// <param name="element"></param>
        /// <param name="transform">Трансформация связанного файла</param>
        /// <exception cref="ArgumentNullException"></exception>
        public ElementModel(Element element, Transform transform)
            : this(element, new TransformModel(transform)) {
        }

        public ElementModel(Element element, TransformModel transformModel) {
            if(element is null) { throw new ArgumentNullException(nameof(element)); }
            if(transformModel is null) { throw new ArgumentNullException(nameof(transformModel)); }

            Id = element.Id;
            Name = element.Name;
            Category = element.Category?.Name;
            Level = RevitRepository.GetLevelName(element);
            DocumentName = RevitRepository.GetDocumentName(element.Document);
            TransformModel = transformModel;
        }


        public ElementId Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Level { get; set; }
        public string DocumentName { get; set; }
        public TransformModel TransformModel { get; set; } = new TransformModel(Transform.Identity);


        public Element GetElement(IEnumerable<DocInfo> docInfos) {
            if(docInfos is null) { throw new ArgumentNullException(nameof(docInfos)); }
            if(docInfos.Any(item => item is null)) { throw new ArgumentNullException(nameof(docInfos)); }

            return GetDocInfo(docInfos)?.Doc.GetElement(Id);
        }

        public DocInfo GetDocInfo(IEnumerable<DocInfo> docInfos) {
            if(docInfos is null) { throw new ArgumentNullException(nameof(docInfos)); }
            if(docInfos.Any(item => item is null)) { throw new ArgumentNullException(nameof(docInfos)); }

            if(TransformModel is null) {
                return docInfos.FirstOrDefault(item => item.Name.Equals(DocumentName));
            } else {
                var docsWithTheSameTitle = docInfos
                    .Where(item => item.Name.Equals(DocumentName))
                    .ToArray();
                // если в активном документе есть дублирующиеся связи,
                // то сначала пытаемся найти нужный экземпляр связи по трансформации,
                // если поиск не удался, то берем первый попавшийся экземпляр связи
                return docsWithTheSameTitle
                    .FirstOrDefault(item => TransformModel.IsAlmostEqualTo(item.Transform))
                    ?? docsWithTheSameTitle.FirstOrDefault();
            }
        }

        public Transform GetTransform() {
            return TransformModel.GetTransform();
        }

        public IList<Solid> GetSolids(IEnumerable<DocInfo> documents) {
            return GetElement(documents)?.GetSolids()
                ?? throw new ArgumentException($"Элемент с Id={Id} не найден в заданных документах");
        }

        public override bool Equals(object obj) {
            return Equals(obj as ElementModel);
        }

        public override int GetHashCode() {
            int hashCode = 426527819;
            hashCode = hashCode * -1521134295 + EqualityComparer<ElementId>.Default.GetHashCode(Id);
            hashCode = hashCode * -1521134295 + EqualityComparer<TransformModel>.Default.GetHashCode(TransformModel);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Category);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Level);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(DocumentName);
            return hashCode;
        }

        public bool Equals(ElementModel other) {
            if(ReferenceEquals(null, other)) { return false; }
            if(ReferenceEquals(this, other)) { return true; }

            return other != null
                && Id == other.Id
                && Name == other.Name
                && Category == other.Category
                && Level == other.Level
                && DocumentName == other.DocumentName
                && Equals(TransformModel, other.TransformModel);
        }
    }
}
