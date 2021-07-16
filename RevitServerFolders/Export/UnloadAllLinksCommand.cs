using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitServerFolders.Export {
    internal class UnloadAllLinksCommand {
        public string SourceFolderName { get; set; }

        public void Execute() {
            if(string.IsNullOrEmpty(SourceFolderName)) {
                throw new InvalidOperationException("Перед использованием укажите папку с файлами Revit.");
            }

            DocumentExtensions.UnloadAllLinks(Directory.GetFiles(SourceFolderName, ".rvt"));
        }
    }
}
