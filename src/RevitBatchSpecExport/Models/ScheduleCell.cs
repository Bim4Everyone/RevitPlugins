using System.Globalization;
using System.Linq;

using Autodesk.Revit.DB;

using ClosedXML.Excel;

namespace RevitBatchSpecExport.Models;

internal class ScheduleCell {
    private readonly TableSectionData _sectionData;
    private readonly Document _doc;
    private readonly int _row;
    private readonly int _column;
    private readonly string _value;

    public ScheduleCell(Document document, TableSectionData sectionData, int row, int column, string value) {
        _doc = document;
        _sectionData = sectionData;
        _row = row;
        _column = column;
        _value = value;
    }

    public string Value => _value;
    public TableCellStyle TableCellStyle => _sectionData.GetTableCellStyle(_row, _column);
    public CellType CellType => _sectionData.GetCellType(_row, _column);
    
    public void SetCellStyle(IXLCell excelCell) {
        var cellStyle = TableCellStyle;

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

    public void ExportCell(IXLCell excelCell) {
        var cellType = CellType;

        switch(cellType) {
            case CellType.Text:
#if REVIT_2024_OR_GREATER
            case CellType.CustomField:
#endif
                SetTextValue(excelCell, Value);
                break;
            case CellType.Graphic:
                SetTextValue(excelCell, string.Empty);
                break;
            case CellType.Parameter:
            case CellType.Inherited:
            case CellType.ParameterText:
            case CellType.CombinedParameter:
            case CellType.CalculatedValue:
                SetValue(excelCell, Value);
                break;
            default:
                SetTextValue(excelCell, Value);
                break;
        }
    }

    private void SetValue(IXLCell cell, string value) {
        // Некоторые значения могут быть числовыми, но за счет дополнительного текста
        // (например единиц измерения) не могут быть преобразованы в число.
        // Также числовые значения могут быть разделены ".", из-за чего не будут корректно преобразованы.
        var decimalSymbol = _doc.GetUnits().DecimalSymbol;

        NumberFormatInfo formatInfo;
        if(decimalSymbol == DecimalSymbol.Dot) {
            formatInfo = CultureInfo.InvariantCulture.NumberFormat;
        } else {
            formatInfo = new CultureInfo("ru-RU").NumberFormat;
        }
        char separator = formatInfo.CurrencyDecimalSeparator[0];

        if(double.TryParse(value, NumberStyles.Any, formatInfo, out double doubleValue)) {
            cell.Value = doubleValue;

            if(value.Contains(separator)) {
                int length = value.Split(separator).Last().Length;
                int fullLength = length + 2;
                cell.Style.NumberFormat.Format = "0.".PadRight(fullLength, '0');
            }

        } else {
            SetTextValue(cell, value);
        }
    }

    private void SetTextValue(IXLCell cell, string text) {
        cell.Value = text;
    }
}

