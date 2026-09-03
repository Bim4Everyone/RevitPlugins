using System.Collections.Generic;
using System.Linq;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Filtration;

using ElementModel = RevitClashDetective.Models.Clashes.ElementModel;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig;
/// <summary>
/// Модель представления для фильтра по элементам конструкций из связей
/// </summary>
internal class StructureLinksSearchSetViewModel : SearchSetViewModel {
    public StructureLinksSearchSetViewModel(
        RevitRepository revitRepository, CategoryFilter filter, bool inverted)
        : base(revitRepository, filter, inverted) {
    }

    private protected override void InitializeGrid() {
        var elements = new List<ElementModel>();
        string[] linkStructureDocs = _revitRepository.GetSelectedRevitLinks()
            .Select(c => RevitRepository.GetDocumentName(c.GetLinkDocument()))
            .ToArray();
        var docInfos = _revitRepository.DocInfos
            .Where(d => linkStructureDocs.Contains(d.Name))
            .ToArray();
        var categoryIds = GetCategoryIds();
        foreach(var docInfo in docInfos) {
            var elems = _revitRepository
                .GetFilteredElements(docInfo.Doc, categoryIds, GetRevitFilter(docInfo.Doc))
                .Where(item => item != null && item.IsValidObject)
                .ToList();
            elements.AddRange(elems.Select(item => new ElementModel(item, docInfo.Transform)));
        }

        Grid = new GridControlViewModel(_revitRepository, elements);
    }
}
