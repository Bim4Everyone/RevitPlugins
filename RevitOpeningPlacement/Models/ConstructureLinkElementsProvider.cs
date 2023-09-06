using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.Models {
    internal class ConstructureLinkElementsProvider : IConstructureLinkElementsProvider {
        private readonly Document _document;

        private readonly Transform _transform;

        private readonly ICollection<ElementId> _elementIds;

        private readonly ICollection<OpeningReal> _openingsReal;


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
            _openingsReal = GetOpeningsReal(_document);
        }


        public Document Document => _document;

        public Transform DocumentTransform => _transform;

        public ICollection<ElementId> GetConstructureElementIds() {
            return _elementIds;
        }

        public ICollection<OpeningReal> GetOpeningsReal() {
            return _openingsReal;
        }

        private ICollection<ElementId> GetElementIds(Document document) {
            return new FilteredElementCollector(document)
                .WhereElementIsNotElementType()
                .WherePasses(FiltersInitializer.GetFilterByAllUsedStructureCategories())
                .ToElementIds();
        }

        private ICollection<OpeningReal> GetOpeningsReal(Document document) {
            return new FilteredElementCollector(document)
                .WhereElementIsNotElementType()
                .WherePasses(FiltersInitializer.GetFilterByAllUsedOpeningsCategories())
                .OfClass(typeof(FamilyInstance))
                .Cast<FamilyInstance>()
                .Where(famInst => famInst.Host != null)
                .Where(famInst => famInst.Symbol.FamilyName.Contains("Отв"))
                .Select(famInst => new OpeningReal(famInst))
                .ToHashSet();
        }
    }
}
