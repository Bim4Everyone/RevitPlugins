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
            revitRepository.InitializeDocInfos();

            var loaders = new List<IClashesLoader>() {
                new RevitClashesLoader(revitRepository, path),
                new PluginClashesLoader(path),
                new NavisHtmlClashesLoader(revitRepository, path)
            };

            var loader = loaders.FirstOrDefault(item => item.IsValid());
            if(loader == null) {
                throw new ArgumentException("Неверный формат файла.");
            }

            var docNames = revitRepository.GetDocuments().Select(item => item.Title).ToList();

            return loader.GetClashes()
                         .Select(item=>item.SetRevitRepository(revitRepository))
                         .Where(item => item.IsValid(docNames));
        }
    }
}