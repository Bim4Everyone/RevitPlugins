using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models;
internal class ConstructureLinkElementsProvider : IConstructureLinkElementsProvider {
    private readonly ICollection<ElementId> _elementIds;

    private readonly ICollection<IOpeningReal> _openingsReal;


    /// <summary>
    /// Конструктор обертки провайдера элементов конструкций из связанного файла
    /// </summary>
    /// <param name="linkDocument">Связанный файл с конструкциями</param>
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
    public ConstructureLinkElementsProvider(RevitRepository revitRepository, RevitLinkInstance linkDocument) {
        if(revitRepository is null) {
            throw new ArgumentNullException(nameof(revitRepository));
        }

        if(linkDocument is null) {
            throw new ArgumentNullException(nameof(linkDocument));
        }

        Document = linkDocument.GetLinkDocument();
        DocumentTransform = linkDocument.GetTransform();
        _elementIds = GetElementIds(revitRepository, Document);
        _openingsReal = GetOpeningsReal(revitRepository, Document);
    }


    public Document Document { get; }

    public Transform DocumentTransform { get; }

    public ICollection<ElementId> GetConstructureElementIds() {
        return _elementIds;
    }

    public ICollection<IOpeningReal> GetOpeningsReal() {
        return _openingsReal;
    }

    private ICollection<ElementId> GetElementIds(RevitRepository revitRepository, Document document) {
        return revitRepository is null
            ? throw new ArgumentNullException(nameof(revitRepository))
            : document is null ? throw new ArgumentNullException(nameof(document)) : revitRepository.GetConstructureElementsIds(document);
    }

    private ICollection<IOpeningReal> GetOpeningsReal(RevitRepository revitRepository, Document document) {
        if(revitRepository is null) {
            throw new ArgumentNullException(nameof(revitRepository));
        }

        return document is null
            ? throw new ArgumentNullException(nameof(document))
            : (ICollection<IOpeningReal>) (RevitRepository.GetBimModelPartsService().InAnyBimModelParts(document, BimModelPart.ARPart)
            ? revitRepository.GetRealOpeningsAr(document).ToArray<IOpeningReal>()
            : revitRepository.GetRealOpeningsKr(document).ToArray<IOpeningReal>());
    }
}
