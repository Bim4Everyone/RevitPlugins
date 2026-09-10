using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Filtration;

using ElementModel = RevitClashDetective.Models.Clashes.ElementModel;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig;
/// <summary>
/// Модель представления для фильтра по элементам ВИС из активного документа
/// </summary>
internal class ActiveDocSearchSetViewModel : SearchSetViewModel {
    public ActiveDocSearchSetViewModel(RevitRepository revitRepository, CategoryFilter filter, bool inverted)
        : base(revitRepository, filter, inverted) {
    }

    private protected override void InitializeGrid() {
        var elements = new List<ElementModel>();
        var doc = _revitRepository.Doc;
        var elems = _revitRepository.GetFilteredElements(doc, GetCategoryIds(), GetRevitFilter(doc))
            .Where(item => item != null && item.IsValidObject)
            .ToList();
        elements.AddRange(elems.Select(item => new ElementModel(item, Transform.Identity)));

        Grid = new GridControlViewModel(_revitRepository, elements);
    }
}
