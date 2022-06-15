using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Clashes;

namespace RevitClashDetective.Models.ClashDetection {
    internal class ProvidersClashDetector {
        private readonly RevitRepository _revitRepository;
        private readonly IProvider _firstProvider;
        private readonly IProvider _secondProvider;

        public ProvidersClashDetector(RevitRepository revitRepository, IProvider firstProvider, IProvider secondProvider) {
            _revitRepository = revitRepository;
            _firstProvider = firstProvider;
            _secondProvider = secondProvider;
        }

        public List<ClashModel> GetClashes() {
            List<ClashModel> clashes = new List<ClashModel>();

            var mainElements = _firstProvider.GetElements();
            if(mainElements.Count == 0) {
                return clashes;
            }

            var resultTransform = _firstProvider.MainTransform.GetTransitionMatrix(_secondProvider.MainTransform);

            foreach(var element in _secondProvider.GetElements()) {
                var solids = _secondProvider.GetSolids(element).Select(item => SolidUtils.CreateTransformed(item, resultTransform)).ToList();
                if(solids == null) {
                    continue;
                }
                foreach(var solid in solids) {

                    clashes.AddRange(new FilteredElementCollector(_firstProvider.Doc, mainElements.Select(item => item.Id).ToList())
                    .WherePasses(new BoundingBoxIntersectsFilter(solid.GetOutline()))
                    .WherePasses(new ElementIntersectsSolidFilter(solid))
                    .Where(item => item.Id != element.Id)
                    .Select(item => new ClashModel(_revitRepository, item, element)));
                }
            }

            return clashes;
        }
    }
}
