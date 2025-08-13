using System;

using Autodesk.Revit.DB;
using ClosedXML.Excel;

namespace RevitExportSpecToExcel.Models;

public class ScheduleToExcelConverter {
    private readonly IXLWorksheet _worksheet;
    private readonly ViewSchedule _schedule;
    private readonly int _numberOfColumns;
    private readonly TableData _tableData;

    public ScheduleToExcelConverter(IXLWorksheet worksheet, ViewSchedule schedule) {
        _worksheet = worksheet;
        _schedule = schedule;

        _tableData = _schedule.GetTableData();
        var tableSection = _tableData.GetSectionData(SectionType.Body);
        _numberOfColumns = tableSection.NumberOfColumns;
    }

    public void Convert() {
        AlignCells();
        ExportSection(SectionType.Header);
        ExportSection(SectionType.Body);
    }

    private void AlignCells() {
        var tableSection = _tableData.GetSectionData(SectionType.Body);
        int numberOfColumns = tableSection.NumberOfColumns;

        for(int i = 0; i < numberOfColumns; i++) {
            double width = tableSection.GetColumnWidth(i);
            width = UnitUtils.ConvertFromInternalUnits(width, UnitTypeId.Millimeters);
            _worksheet.Column(i + 1).Width = width;

        }
    }

    private void ExportSection(SectionType sectionType) {
        var tableSection = _tableData.GetSectionData(sectionType);

        int numberOfRows = tableSection.NumberOfRows;
        int firstRowNumber = tableSection.FirstRowNumber;
        int firstColumNumber = tableSection.FirstColumnNumber;

        for(int i = firstRowNumber; i < numberOfRows; i++) {
            for(int j = firstColumNumber; j < _numberOfColumns; j++) {

                IXLCell excelCell = _worksheet.Cell(i + 1, j + 1);
                var cellValue = tableSection.GetCellText(i, j);
                
                ExportCell(excelCell, cellValue);
            }
        }
    }

    private void ExportCell(IXLCell cell, string value) {

        try {
            double doubleValue = double.Parse(value);
            cell.Value = value;
        }
        catch {

        }
    }
}
