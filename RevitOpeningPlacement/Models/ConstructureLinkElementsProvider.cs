using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models {
    internal class ConstructureLinkElementsProvider : IConstructureLinkElementsProvider {
        private readonly Document _document;

        private readonly Transform _transform;

        private readonly ICollection<ElementId> _elementIds;


        /// <summary>
        /// Конструктор обертки провайдера элементов конструкций из связанного файла
        /// </summary>
        /// <param name="linkDocument">Связанный файл с конструкциями</param>
        /// <exception cref="ArgumentNullException"></exception>
        public ConstructureLinkElementsProvider(RevitLinkInstance linkDocument) {
            if(linkDocument is null) {
                throw new ArgumentNullException(nameof(linkDocument));
            }

            _document = linkDocument.GetLinkDocument();
            _transform = linkDocument.GetTransform();
            _elementIds = GetElementIds(_document);
        }


        public Document Document => _document;

        public Transform DocumentTransform => _transform;

        public ICollection<ElementId> GetElementIds() {
            return _elementIds;
        }


        private ICollection<ElementId> GetElementIds(Document document) {
            return new FilteredElementCollector(document)
                .WhereElementIsNotElementType()
                .WherePasses(FiltersInitializer.GetFilterByAllUsedStructureCategories())
                .ToElementIds();
        }
    }
}
