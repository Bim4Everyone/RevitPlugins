using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using ClosedXML.Excel;

using DocumentFormat.OpenXml.Wordprocessing;

namespace RevitExportSpecToExcel.Models
{
    internal class ExcelExporter {
        public void ExportSchedulesToExcel(string path, IEnumerable<ViewSchedule> schedules, bool asOneFile) {
            if(asOneFile) {
                schedules.Select(x => x.Definition);
            }

            
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(schedules.First().Name);

            workbook.SaveAs(path);
        }
    }
}
