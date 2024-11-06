using System;
using System.Linq;

using RevitOpeningPlacement.Models;

namespace RevitOpeningPlacement.Services {
    /// <summary>
    /// Класс для назначения используемых типов связей по разделам проектирования,
    /// настроенным через <see cref="IDocTypesProvider"/>.
    /// </summary>
    internal class DocTypeLinksSetter : IRevitLinkTypesSetter {
        private readonly RevitRepository _revitRepository;
        private readonly IDocTypesProvider _docTypesProvider;
        private readonly IDocTypesHandler _docTypesHandler;

        public DocTypeLinksSetter(
            RevitRepository revitRepository,
            IDocTypesProvider docTypesProvider,
            IDocTypesHandler docTypesHandler) {

            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
            _docTypesProvider = docTypesProvider ?? throw new ArgumentNullException(nameof(docTypesProvider));
            _docTypesHandler = docTypesHandler ?? throw new ArgumentNullException(nameof(docTypesHandler));
        }


        public void SetRevitLinkTypes() {
            var docTypes = _docTypesProvider.GetDocTypes();
            var linkTypes = _revitRepository.GetAllRevitLinkTypes()
                .Where(link => docTypes.Contains(_docTypesHandler.GetDocType(link)));
            _revitRepository.SetRevitLinkTypesToUse(linkTypes);
        }
    }
}
