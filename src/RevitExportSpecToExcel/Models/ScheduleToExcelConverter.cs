using System;

using Autodesk.Revit.DB;
using ClosedXML.Excel;

namespace RevitExportSpecToExcel.Models;

public class ScheduleToExcelConverter {
    private readonly IXLWorksheet _worksheet;
    private readonly ViewSchedule _schedule;
    private readonly int _numberOfColumns;
    private readonly TableData _tableData;
    
    private int _currentColumn;
    private int _currentRow;

    public ScheduleToExcelConverter(IXLWorksheet worksheet, ViewSchedule schedule) {
        _worksheet = worksheet;
        _schedule = schedule;

        _tableData = _schedule.GetTableData();
        var tableSection = _tableData.GetSectionData(SectionType.Body);
        _numberOfColumns = tableSection.NumberOfColumns;
    }

    public void Convert() {
        _currentColumn = 1;
        _currentRow = 1;

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
        int firstColumnNumber = tableSection.FirstColumnNumber;

        for(int i = firstRowNumber; i < firstRowNumber + numberOfRows; i++) {
            double rowHeight = tableSection.GetRowHeightInPixels(i);
            _worksheet.Row(i + 1).Height = rowHeight;

            for(int j = firstColumnNumber; j < _numberOfColumns + firstColumnNumber; j++) {

                IXLCell excelCell = _worksheet.Cell(_currentRow, _currentColumn);
                var cellValue = _schedule.GetCellText(sectionType, i, j);
                var cellStyle = tableSection.GetTableCellStyle(i, j);

                ExportCell(excelCell, cellValue, cellStyle);

                _currentColumn++;

            }

            _currentRow++;
            _currentColumn = 1;
        }
    }

    private void ExportCell(IXLCell cell, string value, TableCellStyle cellStyle) {
        cell.Style.Font.Bold = cellStyle.IsFontBold;
        cell.Style.Font.Italic = cellStyle.IsFontItalic;
        cell.Style.Font.Underline = cellStyle.IsFontUnderline ? XLFontUnderlineValues.Single : XLFontUnderlineValues.None;
        cell.Style.Font.FontSize = cellStyle.TextSize;
        cell.Style.Font.FontName = cellStyle.FontName;

        cell.Style.Alignment.Horizontal = cellStyle.FontHorizontalAlignment switch {
            HorizontalAlignmentStyle.Left => XLAlignmentHorizontalValues.Left,
            HorizontalAlignmentStyle.Center => XLAlignmentHorizontalValues.Center,
            HorizontalAlignmentStyle.Right => XLAlignmentHorizontalValues.Right,
            _ => cell.Style.Alignment.Horizontal
        };

        cell.Style.Alignment.Vertical = cellStyle.FontVerticalAlignment switch {
            VerticalAlignmentStyle.Bottom => XLAlignmentVerticalValues.Bottom,
            VerticalAlignmentStyle.Middle => XLAlignmentVerticalValues.Center,
            VerticalAlignmentStyle.Top => XLAlignmentVerticalValues.Top,
            _ => cell.Style.Alignment.Vertical
        };

        cell.Value = value;
    }
}
