using Autodesk.Revit.DB;

using ClosedXML.Excel;

using dosymep.Bim4Everyone.SimpleServices;

namespace RevitExportSpecToExcel.Models;

public class ScheduleToExcelConverter {
    private readonly IRevitParamFactory _paramFactory;

    private IXLWorksheet _worksheet;
    private ViewSchedule _schedule;
    private TableData _tableData;

    private int _currentColumn;
    private int _currentRow;

    public ScheduleToExcelConverter(IRevitParamFactory paramFactory) {
        _paramFactory = paramFactory;
    }

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

                    ExportCell(excelCell, scheduleCell);
                    SetCellStyle(excelCell, scheduleCell);

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

    private void SetCellStyle(IXLCell excelCell, ScheduleCell scheduleCell) {
        var cellStyle = scheduleCell.TableCellStyle;

        excelCell.Style.Font.Bold = cellStyle.IsFontBold;
        excelCell.Style.Font.Italic = cellStyle.IsFontItalic;
        excelCell.Style.Font.Underline = cellStyle.IsFontUnderline 
            ? XLFontUnderlineValues.Single 
            : XLFontUnderlineValues.None;
        excelCell.Style.Font.FontSize = cellStyle.TextSize;
        excelCell.Style.Font.FontName = cellStyle.FontName;

        excelCell.Style.Alignment.Horizontal = cellStyle.FontHorizontalAlignment switch {
            HorizontalAlignmentStyle.Left => XLAlignmentHorizontalValues.Left,
            HorizontalAlignmentStyle.Center => XLAlignmentHorizontalValues.Center,
            HorizontalAlignmentStyle.Right => XLAlignmentHorizontalValues.Right,
            _ => XLAlignmentHorizontalValues.Center
        };

        excelCell.Style.Alignment.Vertical = cellStyle.FontVerticalAlignment switch {
            VerticalAlignmentStyle.Bottom => XLAlignmentVerticalValues.Bottom,
            VerticalAlignmentStyle.Middle => XLAlignmentVerticalValues.Center,
            VerticalAlignmentStyle.Top => XLAlignmentVerticalValues.Top,
            _ => XLAlignmentVerticalValues.Center
        };
    }

    private void ExportCell(IXLCell excelCell, ScheduleCell scheduleCell) {
        var cellType = scheduleCell.CellType;

        switch(cellType) {
            case CellType.Text:
#if REVIT_2023_OR_GREATER
            case CellType.CustomField:
#endif
                SetTextValue(excelCell, scheduleCell.Value);
                break;
            case CellType.Graphic:
                SetTextValue(excelCell, "");
                break;
            case CellType.Parameter:
            case CellType.ParameterText:
                SetCellFromParameter(excelCell, scheduleCell);
                break;
            case CellType.Inherited:
            case CellType.CombinedParameter:
            case CellType.CalculatedValue:
                SetDoubleValue(excelCell, scheduleCell.Value);
                break;
            default:
                SetTextValue(excelCell, scheduleCell.Value);
                break;
        }     
    }

    private void SetCellFromParameter(IXLCell excelCell, ScheduleCell scheduleCell) {
        var paramId = scheduleCell.CellParamId;
        var parameter = _paramFactory.Create(_schedule.Document, paramId);
        var paramStorageType = parameter.StorageType;

        if(paramStorageType == StorageType.Double || paramStorageType == StorageType.Integer) {
            SetDoubleValue(excelCell, scheduleCell.Value);
        } else {
            excelCell.Value = scheduleCell.Value;
        }
    }

    private void SetDoubleValue(IXLCell cell, string text) {
        if(double.TryParse(text,out double result)) {
            cell.Value = result;
            cell.Style.NumberFormat.Format = "0";
        } else {
            cell.Value = text;
        }
    }

    private void SetTextValue(IXLCell cell, string text) {
        cell.Value = text;
    }
}
