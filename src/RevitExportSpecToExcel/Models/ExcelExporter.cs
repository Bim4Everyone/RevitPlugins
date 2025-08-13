using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using ClosedXML.Excel;

namespace RevitExportSpecToExcel.Models
{
    internal class ExcelExporter {
        public IReadOnlyCollection<char> ProhibitedExcelChars { get; } = new ReadOnlyCollection<char>(new char[] { '\\', '/', '?', ':', '*', '[', ']', '\'' });
        public int DocNameMaxLength => 31;

        public void ExportSchedulesToExcel(string folderPath, IEnumerable<ViewSchedule> schedules, bool asOneFile) {
            if(asOneFile) {
                using var workbook = new XLWorkbook();
                foreach(var schedule in schedules) {
                    string sheetName = GenerateSheetName(schedule.Name);
                    var worksheet = workbook.Worksheets.Add(sheetName);

                    var converter = new ScheduleToExcelConverter(worksheet, schedule);
                    converter.Convert();
                }

                string fullPath = GenerateFullPath(folderPath, schedules.First().Name);
                workbook.SaveAs(fullPath);

            } else {
                foreach(var schedule in schedules) {
                    using var workbook = new XLWorkbook();
                    string sheetName = GenerateSheetName(schedule.Name);
                    var worksheet = workbook.Worksheets.Add(sheetName);

                    var converter = new ScheduleToExcelConverter(worksheet, schedule);
                    converter.Convert();

                    string fullPath = GenerateFullPath(folderPath, schedule.Name);
                    workbook.SaveAs(fullPath);
                }
            }
        }

        private string GenerateFullPath(string folderPath, string fileName) {
            if(string.IsNullOrEmpty(folderPath)) {
                throw new ArgumentNullException(folderPath);
            }

            const string fileExtension = ".xlsx";

            string filePath = $"{folderPath}\\{fileName}{fileExtension}";
            if(File.Exists(filePath)) {
                string suffix = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
                filePath = $"{folderPath}\\{fileName}_{suffix}{fileExtension}";
            }
            return filePath;
        }

        private string GenerateSheetName(string name) {
            var charsToRemove = ProhibitedExcelChars;
            string trimName = new string(name.Trim().Take(DocNameMaxLength).ToArray()).Trim();
            foreach(char charToRemove in charsToRemove) {
                trimName = trimName.Replace(charToRemove, '_');
            }
            return trimName;
        }
    }
}
