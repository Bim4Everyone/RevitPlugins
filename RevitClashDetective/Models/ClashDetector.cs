using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitClashDetective.Models {
    internal class ClashDetector {
        private readonly List<IProvider> mainDocProviders;
        private readonly List<IProvider> otherProviders;

        public ClashDetector(List<IProvider> mainDocProviders, List<IProvider> otherProviders) {
            this.mainDocProviders = mainDocProviders;
            this.otherProviders = otherProviders;
        }

        public IEnumerable<ClashModel> FindClashes(Document doc) {
            var mainElements = mainDocProviders.SelectMany(item => item.GetElements()).ToList();
            if(mainElements.Count == 0)
                yield break;
            foreach(var provider in otherProviders) {
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
                                yield return new ClashModel() {
                                    MainElement = clashElement,
                                    OtherElement = element
                                };
                            }
                        }
                    }
                }
            }
        }
    }
}
