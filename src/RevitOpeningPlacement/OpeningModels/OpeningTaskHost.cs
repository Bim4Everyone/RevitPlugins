using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitClashDetective.Models.Clashes;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.OpeningModels {
    internal class OpeningTaskHost : IOpeningTaskHost, ISelectorAndHighlighter, IEquatable<OpeningTaskHost> {
        private const string _krModelPartParam = "обр_ФОП_Раздел проекта";
        private readonly Element _element;

        public OpeningTaskHost(Element element) {
            _element = element ?? throw new ArgumentNullException(nameof(element));

            Name = element.Name;
            Id = element.Id;
            KrModelPart = element.GetSharedParamValueOrDefault(_krModelPartParam, string.Empty);
        }

        /// <summary>
        /// Создает экземпляр класса с пустыми значениями свойств
        /// </summary>
        public OpeningTaskHost() {
            Name = string.Empty;
            Id = ElementId.InvalidElementId;
            KrModelPart = string.Empty;
        }


        public string Name { get; }

        public ElementId Id { get; }

        public string KrModelPart { get; }

        public override bool Equals(object obj) {
            return Equals(obj as OpeningTaskHost);
        }

        public bool Equals(OpeningTaskHost other) {
            if(ReferenceEquals(other, null)) {
                return false;
            }
            if(ReferenceEquals(this, other)) {
                return true;
            }
            return Id == other.Id;
        }

        public override int GetHashCode() {
            return 2108858624 + EqualityComparer<ElementId>.Default.GetHashCode(Id);
        }

        public ICollection<ElementModel> GetElementsToSelect() {
            return _element is null ? Array.Empty<ElementModel>() : new ElementModel[] { new ElementModel(_element) };
        }

        public Element GetElementToHighlight() {
            return _element;
        }
    }
}
