using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Autodesk.Revit.DB;

using ClosedXML.Excel;

using RevitExportSpecToExcel.Services;

namespace RevitExportSpecToExcel.Models
{
    internal class ExcelExporter {
        private readonly IConstantsProvider _constantsProvider;
        private readonly ScheduleToExcelConverter _scheduleToExcelConverter;

        public ExcelExporter(IConstantsProvider constantsProvider,
                             ScheduleToExcelConverter scheduleToExcelConverter) {
            _constantsProvider = constantsProvider;
            _scheduleToExcelConverter = scheduleToExcelConverter;
        }

        public void ExportSchedules(string folderPath, string documentName, IList<ViewSchedule> schedules, bool asOneFile) {
            if(asOneFile) {
                using var workbook = new XLWorkbook();

                foreach(var schedule in schedules) {
                    string sheetName = GenerateSheetName(schedule.Name);
                    var worksheet = workbook.Worksheets.Add(sheetName);

                    _scheduleToExcelConverter.Convert(worksheet, schedule);
                }
                
                string fullPath = GenerateFullPath(folderPath, documentName);
                workbook.SaveAs(fullPath);

            } else {
                foreach(var schedule in schedules) {
                    using var workbook = new XLWorkbook();

                    string sheetName = GenerateSheetName(schedule.Name);
                    var worksheet = workbook.Worksheets.Add(sheetName);

                    _scheduleToExcelConverter.Convert(worksheet, schedule);

                    string fileName = $"{documentName}_{schedule.Name}";
                    string fullPath = GenerateFullPath(folderPath, fileName);
                    workbook.SaveAs(fullPath);
                }
            }
        }

        private string GenerateFullPath(string folderPath, string fileName) {
            if(string.IsNullOrEmpty(folderPath)) {
                throw new ArgumentNullException(folderPath);
            }

            string filePath = $"{folderPath}\\{fileName}.{_constantsProvider.FileExtension}";
            if(File.Exists(filePath)) {
                string suffix = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
                filePath = $"{folderPath}\\{fileName}_{suffix}.{_constantsProvider.FileExtension}";
            }
            return filePath;
        }

        private string GenerateSheetName(string name) {
            var charsToRemove = _constantsProvider.ProhibitedExcelChars;
            string trimName = new string(name.Trim().Take(_constantsProvider.DocNameMaxLength).ToArray()).Trim();
            foreach(char charToRemove in charsToRemove) {
                trimName = trimName.Replace(charToRemove, '_');
            }
            return trimName;
        }
    }
}
