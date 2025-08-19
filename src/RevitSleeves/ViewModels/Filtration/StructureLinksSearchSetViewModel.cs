using System.Collections.Generic;
using System.Linq;

using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Models.FilterGenerators;
using RevitClashDetective.Models.FilterModel;

using RevitSleeves.Models;

namespace RevitSleeves.ViewModels.Filtration;

internal class StructureLinksSearchSetViewModel : SearchSetViewModel {
    public StructureLinksSearchSetViewModel(
        RevitRepository revitRepository,
        Filter filter,
        RevitFilterGenerator generator)
        : base(revitRepository, filter, generator) {
    }

    protected override ICollection<ElementViewModel> GetElements() {
        var elements = new List<ElementViewModel>();
        var structureLinks = _revitRepository.GetStructureLinkInstances();
        foreach(var structureLink in structureLinks) {
            var doc = structureLink.GetLinkDocument();
            var filter = Filter.GetRevitFilter(doc, FilterGenerator);
            var linkElements = _revitRepository.GetClashRevitRepository()
                .GetFilteredElements(doc, Filter.CategoryIds, filter)
                .ToArray();
            elements.AddRange(linkElements.Select(e => new ElementViewModel(
                new ElementModel(e, structureLink.GetTransform()))));
        }
        return elements;
    }
}
