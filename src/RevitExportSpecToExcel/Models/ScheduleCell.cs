using Autodesk.Revit.DB;

namespace RevitExportSpecToExcel.Models;

internal class ScheduleCell {
    private readonly TableSectionData _sectionData;
    private readonly int _row;
    private readonly int _column;
    private readonly string _value;

    public ScheduleCell(TableSectionData sectionData, int row, int column, string value) {
        _sectionData = sectionData;
        _row = row;
        _column = column;
        _value = value;
    }

    public string Value => _value;
    public TableCellStyle TableCellStyle => _sectionData.GetTableCellStyle(_row, _column);
    public CellType CellType => _sectionData.GetCellType(_row, _column);
    public ElementId CellParamId => _sectionData.GetCellParamId(_row, _column);
}

