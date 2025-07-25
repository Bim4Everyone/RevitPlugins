using System.Collections.Generic;
using System.Linq;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;

using RevitCorrectNamingCheck.Helpers;

namespace RevitCorrectNamingCheck.Models;

internal class LinkedFileEnricher {

    private static readonly Dictionary<string, string> _partFileToRnMap = new() {
    { "AR", "AR" },
    { "KR", "KR" },
    { "OV", "OV" },
    { "ITP", "OV" },     // ITP считается частью OV в РН
    { "VK", "VK" },
    { "EOM", "EOM" },
    { "EG", "EOM" },     // EG считается частью EOM в РН
    { "SS", "SS" }
    };


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
        var mappedIdentifiers = identifiers
            .Select(i => _partFileToRnMap.TryGetValue(i, out var mapped) ? mapped : i)
            .Distinct()
            .ToList();

        bool isLink = NamingRulesHelper.IsLinkWorkset(worksetName);

        bool isCurrent = false;
        if (currentPart != null) {
            isCurrent = NamingRulesHelper.MatchesCurrentPart(worksetName, currentPart);
        }
        int matches = mappedIdentifiers.Count(part => NamingRulesHelper.ContainsPart(worksetName, part));

        if(!isLink) {
            return NameStatus.Incorrect;
        }

        if(matches == 1 && isCurrent) {
            return NameStatus.Correct;
        }

        if(matches > 1) {
            return NameStatus.PartialCorrect;
        }

        return NameStatus.None;
    }


    public LinkedFile Enrich(LinkedFile linkedFile) {

        var requiredParts = new[] { "AR", "KR", "OV", "ITP", "VK", "EOM", "EG", "SS" };

        var bimModelParts = _bimModelPartsService
            .GetBimModelParts()
            .Where(p => requiredParts.Contains(p.Id))
            .ToList();

        var bimIdentifiers = bimModelParts
            .SelectMany(p => new[] { p.Id, p.Name })
            .Distinct()
            .ToList();

        linkedFile.FileNameStatus = GetFileNameStatus(linkedFile.Name, bimIdentifiers);

        var currentPart = _bimModelPartsService.GetBimModelPart(linkedFile.Name);

        SetWorksetInfo(linkedFile.TypeWorkset, currentPart, bimIdentifiers);
        SetWorksetInfo(linkedFile.InstanceWorkset, currentPart, bimIdentifiers);

        return linkedFile;
    }
}
