using System;
using System.Linq;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models;

namespace RevitOpeningPlacement.Services;
/// <summary>
/// Обновляет используемые типы загруженных связей в <see cref="Models.RevitRepository"/> в соответствии с заданными разделами проектирования.<br/>
/// Разделы проектирования задаются через <see cref="Services.IDocTypesProvider"/><br/>
/// </summary>
internal class DocTypeLoadedLinksSetter : IRevitLinkTypesSetter {
    private readonly RevitRepository _revitRepository;
    private readonly IDocTypesProvider _docTypesProvider;
    private readonly IDocTypesHandler _docTypesHandler;

    public DocTypeLoadedLinksSetter(
        RevitRepository revitRepository,
        IDocTypesProvider docTypesProvider,
        IDocTypesHandler docTypesHandler) {

        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _docTypesProvider = docTypesProvider ?? throw new ArgumentNullException(nameof(docTypesProvider));
        _docTypesHandler = docTypesHandler ?? throw new ArgumentNullException(nameof(docTypesHandler));
    }

    /// <summary>
    /// Назначает связи для использования, которые загружены в активный документ и относятся к заданному разделу проектирования.
    /// </summary>
    public void SetRevitLinkTypes() {
        var docTypes = _docTypesProvider.GetDocTypes();
        var linkTypes = _revitRepository.GetAllRevitLinkTypes()
            .Where(link => RevitLinkType.IsLoaded(link.Document, link.Id)
                && docTypes.Contains(_docTypesHandler.GetDocType(link)));
        _revitRepository.SetRevitLinkTypesToUse(linkTypes);
    }
}
