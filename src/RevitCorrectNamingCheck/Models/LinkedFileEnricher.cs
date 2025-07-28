using System.Collections.Generic;
using System.Linq;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;

using RevitCorrectNamingCheck.Helpers;

namespace RevitCorrectNamingCheck.Models;

internal class LinkedFileEnricher {

    private readonly IBimModelPartsService _bimModelPartsService;

    public LinkedFileEnricher(IBimModelPartsService bimModelPartsService) {
        _bimModelPartsService = bimModelPartsService;
    }

    private void SetWorksetInfo(WorksetInfo workset, BimModelPart currentPart, List<string> bimIdentifiers) {
        workset.WorksetNameStatus = GetWorksetNameStatus(workset.Name, currentPart, bimIdentifiers);
    }

    private NameStatus GetFileNameStatus(string name, List<string> bimIdentifiers) {
        int matches = bimIdentifiers.Count(part => NamingRulesHelper.ContainsPart(name, part));

        return matches == 0 ? NameStatus.None : matches == 1 ? NameStatus.Correct : NameStatus.Incorrect;
    }

    private NameStatus GetWorksetNameStatus(string worksetName, BimModelPart currentPart, List<string> identifiers) {
        int matches = identifiers.Count(part => NamingRulesHelper.ContainsPart(worksetName, part));
        bool isCurrent = NamingRulesHelper.MatchesCurrentPart(worksetName, currentPart);
        bool isLink = NamingRulesHelper.IsLinkWorkset(worksetName);

        return matches == 0
            ? NameStatus.None
            : matches == 1 ? isCurrent ? isLink ? NameStatus.Correct : NameStatus.PartialCorrect : NameStatus.None : NameStatus.Incorrect;
    }

    public LinkedFile Enrich(LinkedFile linkedFile) {

        var bimModelParts = _bimModelPartsService.GetBimModelParts().ToList();

        var bimIdentifiers = bimModelParts
            .SelectMany(p => new[] { p.Id, p.Name })
            .Distinct()
            .ToList();

        linkedFile.FileNameStatus = GetFileNameStatus(linkedFile.Name, bimIdentifiers);

        if(linkedFile.FileNameStatus == NameStatus.Correct) {
            var currentPart = _bimModelPartsService.GetBimModelPart(linkedFile.Name);

            SetWorksetInfo(linkedFile.TypeWorkset, currentPart, bimIdentifiers);
            SetWorksetInfo(linkedFile.InstanceWorkset, currentPart, bimIdentifiers);
        }

        return linkedFile;
    }
}
