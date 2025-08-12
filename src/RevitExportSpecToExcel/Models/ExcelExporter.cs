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
            TableData tableData = schedule.GetTableData();
            var tableSection = tableData.GetSectionData(SectionType.Body);
            //var mergedCell = tableSection.GetMergedCell();

            int headerRows = tableData.GetSectionData(SectionType.Header).NumberOfRows;
            int rowCount = tableSection.NumberOfRows;
            int colCount = tableSection.NumberOfColumns;

            AlignCells(worksheet, tableData);

            // Заполняем Excel данными
            for(int i = 0; i < rowCount; i++) {
                for(int j = 0; j < colCount; j++) {
                    var cell = tableSection.GetCellText(i, j);
                    worksheet.Cell(i + 1, j + 1).Value = cell;
                }
            }
        }

        private void AlignCells(IXLWorksheet worksheet, TableData tableData) {
            var tableSection = tableData.GetSectionData(SectionType.Body);
            int numberOfColumns = tableSection.NumberOfColumns;

            for(int i = 0; i < numberOfColumns; i++) {
                double width = tableSection.GetColumnWidth(i);
                width = UnitUtils.ConvertFromInternalUnits(width, UnitTypeId.Millimeters);
                worksheet.Column(i + 1).Width = width;
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
