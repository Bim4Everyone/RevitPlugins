using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.FilterGenerators;

using RevitClashDetective.Models.FilterModel;

using RevitOpeningPlacement.Models;

using ElementModel = RevitClashDetective.Models.Clashes.ElementModel;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig;
/// <summary>
/// Модель представления для фильтра по элементам ВИС из активного документа
/// </summary>
internal class ActiveDocSearchSetViewModel : SearchSetViewModel {
    public ActiveDocSearchSetViewModel(
        RevitRepository revitRepository, Filter filter, RevitFilterGenerator generator)
        : base(revitRepository, filter, generator) {
    }

    private protected override void InitializeGrid() {
        var elements = new List<ElementModel>();
        var doc = _revitRepository.Doc;
        var filter = Filter.GetRevitFilter(doc, FilterGenerator);
        var elems = _revitRepository.GetFilteredElements(doc, Filter.CategoryIds, filter)
            .Where(item => item != null && item.IsValidObject)
            .ToList();
        elements.AddRange(elems.Select(item => new ElementModel(item, Transform.Identity)));

        Grid = new GridControlViewModel(_revitRepository, Filter, elements);
    }
}
