using System.Collections.Generic;
using System.Linq;

using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Models.ClashDetection;
internal class ClashDetector {
    private readonly RevitRepository _revitRepository;
    private readonly IEnumerable<IProvider> _mainDocProviders;
    private readonly IEnumerable<IProvider> _otherProviders;

    public ClashDetector(RevitRepository revitRepository, IEnumerable<IProvider> mainDocProviders, IEnumerable<IProvider> otherProviders) {
        _revitRepository = revitRepository;
        _mainDocProviders = mainDocProviders;
        _otherProviders = otherProviders;
    }

    public List<ClashModel> FindClashes() {
        List<ClashModel> clashes = [];
        foreach(var mainProvider in _mainDocProviders) {
            foreach(var provider in _otherProviders) {
                var providerClashDetector = new ProvidersClashDetector(_revitRepository, mainProvider, provider);
                clashes.AddRange(providerClashDetector.GetClashes());
            }
        }
        for(int i = 0; i < clashes.Count; i++) {
            clashes[i].Name = $"Конфликт{i + 1}";
        }

        return clashes;
    }
}

internal class ClashesMarker {
    public static IEnumerable<ClashModel> MarkSolvedClashes(
        ICollection<ClashModel> newClashes,
        ICollection<ClashModel> oldClashes) {
        foreach(var newClash in newClashes) {
            var oldClash = oldClashes.FirstOrDefault(item => item.Equals(newClash));
            if(oldClash != null) {
                newClash.ClashStatus = oldClash.ClashStatus;
                newClash.Name = string.IsNullOrWhiteSpace(oldClash.Name) ? newClash.Name : oldClash.Name;
                newClash.Comments = oldClash.Comments;
            }
            yield return newClash;
        }
    }
}
