using System.Collections.Generic;
using System.Linq;

using RevitClashDetective.Models.FilterGenerators;
using RevitClashDetective.Models.FilterModel;

using RevitOpeningPlacement.Models;

using ElementModel = RevitClashDetective.Models.Clashes.ElementModel;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig;
/// <summary>
/// Модель представления для фильтра по элементам конструкций из связей
/// </summary>
internal class StructureLinksSearchSetViewModel : SearchSetViewModel {
    public StructureLinksSearchSetViewModel(
        RevitRepository revitRepository, Filter filter, RevitFilterGenerator generator)
        : base(revitRepository, filter, generator) {
    }

    private protected override void InitializeGrid() {
        var elements = new List<ElementModel>();
        var clashRepo = _revitRepository.GetClashRevitRepository();
        string[] linkStructureDocs = _revitRepository.GetSelectedRevitLinks()
            .Select(c => RevitRepository.GetDocumentName(c.GetLinkDocument()))
            .ToArray();
        var docInfos = _revitRepository.DocInfos
            .Where(d => linkStructureDocs.Contains(d.Name))
            .ToArray();
        foreach(var docInfo in docInfos) {
            var filter = Filter.GetRevitFilter(docInfo.Doc, FilterGenerator);
            var elems = _revitRepository.GetFilteredElements(docInfo.Doc, Filter.CategoryIds, filter)
                .Where(item => item != null && item.IsValidObject)
                .ToList();
            elements.AddRange(elems.Select(item => new ElementModel(item, docInfo.Transform)));
        }

        Grid = new GridControlViewModel(_revitRepository, Filter, elements);
    }
}
