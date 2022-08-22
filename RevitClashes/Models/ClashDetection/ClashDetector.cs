using System.Collections.Generic;
using System.Linq;

using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Models.ClashDetection {
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
            List<ClashModel> clashes = new List<ClashModel>();
            foreach(var mainProvider in _mainDocProviders) {
                foreach(var provider in _otherProviders) {
                    var providerClashDetector = new ProvidersClashDetector(_revitRepository, mainProvider, provider);
                    clashes.AddRange(providerClashDetector.GetClashes());
                }
            }

            return clashes;
        }
    }

    internal class ClashesMarker {
        public static IEnumerable<ClashModel> MarkSolvedClashes(IEnumerable<ClashModel> newClashes, IEnumerable<ClashModel> oldClashes) {
            foreach(var newClash in newClashes) {
                var oldClashe = oldClashes.FirstOrDefault(item => item.Equals(newClash));
                if(oldClashe != null) {
                    newClash.ClashStatus = oldClashe.ClashStatus;
                }
                yield return newClash;
            }
        }
    }
}
