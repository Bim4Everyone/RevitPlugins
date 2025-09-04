using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Revit.Comparators;

namespace RevitBatchPrint.Models {
    internal class RevitExportToPdf : IRevitPrint {
        private readonly Document _document;

        public RevitExportToPdf(Document document) {
            _document = document;
        }

        public void Execute(IReadOnlyCollection<SheetElement> sheets, PrintOptions printOptions) {
#if REVIT_2022_OR_GREATER
            PDFExportOptions exportParams = printOptions.CreateExportParams();

            string directoryName = Path.GetDirectoryName(printOptions.FilePath);
            
            Directory.CreateDirectory(directoryName!);
            _document.Export(directoryName, 
                sheets.Select(item => item.ViewSheet.Id).ToArray(), exportParams);
            
            Process.Start(printOptions.FilePath);
#endif
        }
    }
}
