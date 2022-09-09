using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.ClashDetection;
using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Models.FilterModel;

namespace RevitOpeningPlacement.Models {
    internal class ClashInitializer {
        public static IEnumerable<ClashModel> GetClashes(RevitClashDetective.Models.RevitRepository revitRepository, Filter mepFilter, Filter architectureFilter) {
            var mainProvider = new FilterProvider(revitRepository.Doc, mepFilter, Transform.Identity);
            var otherProviders = revitRepository.DocInfos
                                                .Select(item => new FilterProvider(item.Doc, architectureFilter, item.Transform))
                                                .ToList();
            var clashDetector = new ClashDetector(revitRepository, new[] { mainProvider }, otherProviders);
            return clashDetector.FindClashes();
        }
    }
}
