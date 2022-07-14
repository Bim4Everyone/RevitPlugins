using System.Collections.Generic;

using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Models.ClashDetection {
    internal class ClashDetector {
        private readonly RevitRepository _revitRepository;
        private readonly List<IProvider> _mainDocProviders;
        private readonly List<IProvider> _otherProviders;

        public ClashDetector(RevitRepository revitRepository, List<IProvider> mainDocProviders, List<IProvider> otherProviders) {
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
}
