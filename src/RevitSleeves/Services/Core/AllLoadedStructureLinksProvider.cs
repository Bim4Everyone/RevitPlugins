using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

using RevitSleeves.Exceptions;
using RevitSleeves.Models;

namespace RevitSleeves.Services.Core;
internal class AllLoadedStructureLinksProvider : IStructureLinksProvider {
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;

    public AllLoadedStructureLinksProvider(
        RevitRepository revitRepository,
        ILocalizationService localizationService) {

        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
    }


    public ICollection<RevitLinkInstance> GetLinks() {
        var links = _revitRepository.GetStructureLinkInstances();
        string duplicatedLink = links.GroupBy(l => l.GetLinkDocument().Title).FirstOrDefault(g => g.Count() > 1)?.Key;
        if(!string.IsNullOrEmpty(duplicatedLink)) {
            throw new InvalidOperationException(
                string.Format(_localizationService.GetLocalizedString("Errors.DuplicatedLinks"), duplicatedLink));
        }
        if(links.Count == 0) {
            throw new StructureLinksNotFoundException(
                _localizationService.GetLocalizedString("Errors.CannotFindStructureLinks"));
        }
        return links;
    }

    public string[] GetOpeningFamilyNames() {
        return [.. NamesProvider.FamilyNamesAllOpenings];
    }
}
