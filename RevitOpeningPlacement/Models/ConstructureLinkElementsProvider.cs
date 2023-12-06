using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models {
    internal class ConstructureLinkElementsProvider : IConstructureLinkElementsProvider {
        private readonly Document _document;

        private readonly Transform _transform;

        private readonly ICollection<ElementId> _elementIds;

        private readonly ICollection<IOpeningReal> _openingsReal;


        /// <summary>
        /// Конструктор обертки провайдера элементов конструкций из связанного файла
        /// </summary>
        /// <param name="linkDocument">Связанный файл с конструкциями</param>
        /// <exception cref="ArgumentNullException"></exception>
        public ConstructureLinkElementsProvider(RevitRepository revitRepository, RevitLinkInstance linkDocument) {
            if(revitRepository is null) {
                throw new ArgumentNullException(nameof(revitRepository));
            }

            if(linkDocument is null) {
                throw new ArgumentNullException(nameof(linkDocument));
            }

            _document = linkDocument.GetLinkDocument();
            _transform = linkDocument.GetTransform();
            _elementIds = GetElementIds(revitRepository, _document);
            _openingsReal = GetOpeningsReal(revitRepository, _document);
        }


        public Document Document => _document;

        public Transform DocumentTransform => _transform;

        public ICollection<ElementId> GetConstructureElementIds() {
            return _elementIds;
        }

        public ICollection<IOpeningReal> GetOpeningsReal() {
            return _openingsReal;
        }

        private ICollection<ElementId> GetElementIds(RevitRepository revitRepository, Document document) {
            if(revitRepository is null) {
                throw new ArgumentNullException(nameof(revitRepository));
            }

            if(document is null) {
                throw new ArgumentNullException(nameof(document));
            }

            return revitRepository.GetConstructureElementsIds(document);
        }

        private ICollection<IOpeningReal> GetOpeningsReal(RevitRepository revitRepository, Document document) {
            if(revitRepository is null) {
                throw new ArgumentNullException(nameof(revitRepository));
            }

            if(document is null) {
                throw new ArgumentNullException(nameof(document));
            }

            if(RevitRepository.GetBimModelPartsService().InAnyBimModelParts(document, BimModelPart.ARPart)) {
                return revitRepository.GetRealOpeningsAr(document).ToArray<IOpeningReal>();
            } else {
                return revitRepository.GetRealOpeningsKr(document).ToArray<IOpeningReal>();
            }
        }
    }
}
