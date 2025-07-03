using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;

using RevitSleeves.Models;

namespace RevitSleeves.Services.Core;
internal class AllLoadedStructureLinksProvider : IStructureLinksProvider {
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;
    private readonly IBimModelPartsService _bimModelPartsService;

    public AllLoadedStructureLinksProvider(
        RevitRepository revitRepository,
        ILocalizationService localizationService,
        IBimModelPartsService bimModelPartsService) {

        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _bimModelPartsService = bimModelPartsService ?? throw new ArgumentNullException(nameof(bimModelPartsService));
    }


    public ICollection<RevitLinkInstance> GetLinks() {
        RevitLinkInstance[] links = [.. new FilteredElementCollector(_revitRepository.Document)
            .OfClass(typeof(RevitLinkInstance))
            .OfType<RevitLinkInstance>()
            .Where(link => _bimModelPartsService.InAnyBimModelParts(link, BimModelPart.ARPart, BimModelPart.KRPart)
                && RevitLinkType.IsLoaded(_revitRepository.Document, link.GetTypeId()))];
        string duplicatedLink = links.GroupBy(l => l.Name).FirstOrDefault(g => g.Count() > 1)?.Key;
        if(!string.IsNullOrEmpty(duplicatedLink)) {
            throw new InvalidOperationException(
                string.Format(_localizationService.GetLocalizedString("Errors.DuplicatedLinks"), duplicatedLink);
        }
        if(links.Length == 0) {
            throw new InvalidOperationException(
                _localizationService.GetLocalizedString("Errors.CannotFindStructureLinks"));
        }
        return links;
    }

    public string[] GetOpeningFamilyNames() {
        return [
            NamesProvider.FamilyNameOpeningArRectangleInFloor,
            NamesProvider.FamilyNameOpeningArRectangleInWall,
            NamesProvider.FamilyNameOpeningArRoundInFloor,
            NamesProvider.FamilyNameOpeningArRoundInWall,
            NamesProvider.FamilyNameOpeningKrRectangleInFloor,
            NamesProvider.FamilyNameOpeningKrRectangleInWall,
            NamesProvider.FamilyNameOpeningKrRoundInWall];
    }

    public void SetLinks(ICollection<RevitLinkType> links) {
        // nothing
    }
}
