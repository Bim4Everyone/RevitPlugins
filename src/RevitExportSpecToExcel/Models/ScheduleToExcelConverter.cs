using Autodesk.Revit.DB;

using ClosedXML.Excel;

namespace RevitExportSpecToExcel.Models;

public class ScheduleToExcelConverter {
    private IXLWorksheet _worksheet;
    private ViewSchedule _schedule;
    private TableData _tableData;

    private int _currentColumn;
    private int _currentRow;

    public void Convert(IXLWorksheet worksheet, ViewSchedule schedule) {
        _worksheet = worksheet;
        _schedule = schedule;

        _tableData = _schedule.GetTableData();

        _currentColumn = 1;
        _currentRow = 1;

        AlignCells();
        ExportSection(SectionType.Header);
        ExportSection(SectionType.Body);
    }

    private void ExportSection(SectionType sectionType) {
        var sectionData = _tableData.GetSectionData(sectionType);

        int numberOfRows = sectionData.NumberOfRows;
        int numberOfColumns = sectionData.NumberOfColumns;
        int firstRowNumber = sectionData.FirstRowNumber;
        int firstColumnNumber = sectionData.FirstColumnNumber;

        for(int row = firstRowNumber; row < firstRowNumber + numberOfRows; row++) {
            double rowHeight = sectionData.GetRowHeightInPixels(row);
            _worksheet.Row(row + 1).Height = rowHeight;

            for(int column = firstColumnNumber; column < numberOfColumns + firstColumnNumber; column++) {
                var mergedCells = sectionData.GetMergedCell(row, column);

                if(mergedCells.Top == row && mergedCells.Left == column) {
                    string cellValue = _schedule.GetCellText(sectionType, row, column);
                    ScheduleCell scheduleCell = new ScheduleCell(sectionData, row, column, cellValue);
                    IXLCell excelCell = _worksheet.Cell(_currentRow, _currentColumn);

                    scheduleCell.ExportCell(excelCell);
                    scheduleCell.SetCellStyle(excelCell);

                    int rowRange = mergedCells.Bottom - row;
                    int columnRange = mergedCells.Right - column;
                    if(rowRange > 0 || columnRange > 0) {
                        int startRow = _currentRow;
                        int startColumn = _currentColumn;
                        int endRow = _currentRow + rowRange;
                        int endColumn = _currentColumn + columnRange;
                        _worksheet.Range(startRow, startColumn, endRow, endColumn).Merge();
                    }
                }

                _currentColumn++;
            }

            _currentRow++;
            _currentColumn = 1;
        }
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
}
