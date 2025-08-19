using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Models.FilterGenerators;
using RevitClashDetective.Models.FilterModel;

using RevitSleeves.Models;

namespace RevitSleeves.ViewModels.Filtration;
internal class ActiveDocSearchSetViewModel : SearchSetViewModel {
    public ActiveDocSearchSetViewModel(
        RevitRepository revitRepository,
        Filter filter,
        RevitFilterGenerator generator)
        : base(revitRepository, filter, generator) {
    }


    protected override ICollection<ElementViewModel> GetElements() {
        var filter = Filter.GetRevitFilter(_revitRepository.Document, FilterGenerator);
        return [.. _revitRepository.GetClashRevitRepository()
            .GetFilteredElements(_revitRepository.Document, Filter.CategoryIds, filter)
            .Select(e => new ElementViewModel(new ElementModel(e, Transform.Identity)))];
    }
}
