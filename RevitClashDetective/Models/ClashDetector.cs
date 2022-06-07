using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Clashes;

namespace RevitClashDetective.Models {
    internal class ClashDetector {
        private readonly RevitRepository _revitRepository;
        private readonly List<IProvider> _mainDocProviders;
        private readonly List<IProvider> _otherProviders;

        public ClashDetector(RevitRepository revitRepository, List<IProvider> mainDocProviders, List<IProvider> otherProviders) {
            _revitRepository = revitRepository;
            _mainDocProviders = mainDocProviders;
            _otherProviders = otherProviders;
        }

        public IEnumerable<ClashModel> FindClashes(Document doc) {
            var mainElements = _mainDocProviders.SelectMany(item => item.GetElements()).ToList();
            if(mainElements.Count == 0)
                yield break;
            foreach(var provider in _otherProviders) {
                foreach(var element in provider.GetElements()) {
                    var solids = provider.GetSolids(element);
                    if(solids == null) {
                        continue;
                    }
                    foreach(var solid in solids) {
                        var clashElements = new FilteredElementCollector(doc, mainElements.Select(item => item.Id).ToList())
                        .WherePasses(new BoundingBoxIntersectsFilter(provider.GetOutline(solid)))
                        .WherePasses(new ElementIntersectsSolidFilter(solid))
                        .ToList();
                        foreach(var clashElement in clashElements) {
                            if(clashElement.Id != element.Id) {
                                yield return new ClashModel(_revitRepository, clashElement, element);
                            }
                        }
                    }
                }
            }
        }
    }
}
