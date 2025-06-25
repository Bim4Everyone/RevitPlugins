using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;

using RevitSleeves.Models;

namespace RevitSleeves.Services.Core;
internal class AllLoadedStructureLinksProvider : IStructureLinksProvider {
    private readonly RevitRepository _revitRepository;
    private readonly IBimModelPartsService _bimModelPartsService;

    public AllLoadedStructureLinksProvider(RevitRepository revitRepository, IBimModelPartsService bimModelPartsService) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _bimModelPartsService = bimModelPartsService ?? throw new ArgumentNullException(nameof(bimModelPartsService));
    }


    public ICollection<RevitLinkInstance> GetLinks() {
        return [.. new FilteredElementCollector(_revitRepository.Document)
            .OfClass(typeof(RevitLinkInstance))
            .OfType<RevitLinkInstance>()
            .Where(link => _bimModelPartsService.InAnyBimModelParts(link,
                BimModelPart.ARPart,
                BimModelPart.KRPart))];
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
