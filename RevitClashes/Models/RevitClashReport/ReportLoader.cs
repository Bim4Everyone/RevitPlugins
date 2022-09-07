using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Models.RevitClashReport {
    internal class ReportLoader {
        public static IEnumerable<ClashModel> GetClashes(RevitRepository revitRepository, string path) {
            var loaders = new List<IClashesLoader>() {
                new RevitClashesLoader(revitRepository, path),
                new PluginClashesLoader(path),
                new NavisHtmlClashesLoader(revitRepository, path)
            };

            return loaders.FirstOrDefault(item => item.IsValid())?.GetClashes();
        }
    }
}