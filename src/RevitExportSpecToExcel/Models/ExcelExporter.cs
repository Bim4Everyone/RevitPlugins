using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using ClosedXML.Excel;

namespace RevitExportSpecToExcel.Models
{
    internal class ExcelExporter {
        public void ExportSchedulesToExcel(string folderPath, IEnumerable<ViewSchedule> schedules, bool asOneFile) {
            if(asOneFile) {
                using var workbook = new XLWorkbook();
                foreach(var schedule in schedules) {
                    string sheetName = GenerateSheetName(schedule.Name);
                    var worksheet = workbook.Worksheets.Add(sheetName);

                    FillWorkSheet(schedule, worksheet);
                }

                string fullPath = GenerateFullPath(folderPath, schedules.First().Name);
                workbook.SaveAs(fullPath);

            } else {
                foreach(var schedule in schedules) {
                    using var workbook = new XLWorkbook();
                    string sheetName = GenerateSheetName(schedule.Name);
                    var worksheet = workbook.Worksheets.Add(schedule.Name);

                    FillWorkSheet(schedule, worksheet);

                    string fullPath = GenerateFullPath(folderPath, schedule.Name);
                    workbook.SaveAs(fullPath);
                }
            }
        }

        private void FillWorkSheet(ViewSchedule schedule, IXLWorksheet worksheet) {

        }

        private string GenerateFullPath(string folderPath, string fileName) {
            return $"{folderPath}\\{fileName}.xlsx";
        }


        private string GenerateSheetName(string name) {
            return name.Substring(0, 6);
        }
    }
}
