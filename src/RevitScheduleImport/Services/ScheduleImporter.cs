using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

using Autodesk.Revit.DB;

using ClosedXML.Excel;

using dosymep.Revit;

using RevitScheduleImport.Models;

namespace RevitScheduleImport.Services {
    public class ScheduleImporter {
        private readonly RevitRepository _revitRepository;
        private readonly LengthConverter _lengthConverter;
        private readonly ExcelReader _excelReader;
        private readonly Dictionary<XLThemeColor, System.Drawing.Color> _colorsDictionary;


        public ScheduleImporter(
            RevitRepository revitRepository,
            LengthConverter lengthConverter,
            ExcelReader excelReader) {
            _revitRepository = revitRepository ?? throw new System.ArgumentNullException(nameof(revitRepository));
            _lengthConverter = lengthConverter ?? throw new System.ArgumentNullException(nameof(lengthConverter));
            _excelReader = excelReader ?? throw new System.ArgumentNullException(nameof(excelReader));
            _colorsDictionary = new Dictionary<XLThemeColor, System.Drawing.Color>();
        }


        public void ImportSchedule(
            string path,
            string transactionName,
            BuiltInCategory category,
            out string[] failedWorksheets) {
            if(!File.Exists(path)) {
                throw new FileNotFoundException("Excel файл не найден.", path);
            }
            if(string.IsNullOrWhiteSpace(transactionName)) {
                throw new ArgumentException(nameof(transactionName));
            }

            using(var excelData = _excelReader.ReadExcel(path)) {
                using(Transaction trans = _revitRepository.Document.StartTransaction(transactionName)) {
                    List<string> failedSheets = new List<string>();
                    foreach(var worksheet in excelData.Workbook.Worksheets) {
                        ViewSchedule schedule = default;
                        try {
                            string scheduleName = CreateScheduleName(path, worksheet.Name);
                            schedule = _revitRepository.CreateSchedule(scheduleName, category);
                            WriteData(schedule, worksheet);
                        } catch(Exception ex) when(ex is Autodesk.Revit.Exceptions.ApplicationException
                            || ex is NullReferenceException) {
                            _revitRepository.DeleteSchedule(schedule);
                            failedSheets.Add(worksheet.Name);
                        }
                    }
                    failedWorksheets = failedSheets.ToArray();
                    trans.Commit();
                }
            }
        }


        private string CreateScheduleName(string excelPath, string worksheetName) {
            return $"{Path.GetFileNameWithoutExtension(excelPath)}_{worksheetName}";
        }

        private void WriteData(ViewSchedule schedule, IXLWorksheet worksheet) {
            TableSectionData tableSectionData = CreateTableSectionData(schedule.GetTableData(), worksheet);
            tableSectionData = FillTable(tableSectionData, worksheet);
            RefreshIfNeed(tableSectionData);
        }

        private IXLColumns GetColumns(IXLWorksheet worksheet) {
            IXLColumn lastCol = worksheet.LastColumnUsed(XLCellsUsedOptions.AllContents);
            int lastIndex = lastCol.ColumnNumber();
            return worksheet.Columns(1, lastIndex);
        }

        private IXLRows GetRows(IXLWorksheet worksheet) {
            IXLRow lastRow = worksheet.LastRowUsed(XLCellsUsedOptions.All);
            int lastIndex = lastRow.RowNumber();
            return worksheet.Rows(1, lastIndex);
        }

        /// <summary>
        /// Создает сетку строк и столбцов в заголовке спецификации Revit.
        /// Также устанавливается ширина столбцов и высота строк, объединяются ячейки.
        /// </summary>
        /// <param name="tableData">Таблица спецификации Revit</param>
        /// <param name="worksheet">Лист Excel</param>
        /// <returns>Сетка заголовка таблицы Revit</returns>
        private TableSectionData CreateTableSectionData(TableData tableData, IXLWorksheet worksheet) {
            IXLColumns columns = GetColumns(worksheet);
            IXLRows rows = GetRows(worksheet);

            TableSectionData tableSectionData = tableData.GetSectionData(SectionType.Header);
            tableData.Width = columns.Sum(col => _lengthConverter.ConvertExcelColWidthToInternal(col.Width));
            int columnsCount = columns.Count();
            int i = 0;
            foreach(IXLColumn col in columns) {
                if(i < (columnsCount - 1)) {
                    tableSectionData.InsertColumn(i + 1);
                }
                tableSectionData.SetColumnWidth(i, _lengthConverter.ConvertExcelColWidthToInternal(col.Width));
                i++;
            }

            int rowsCount = rows.Count();
            int j = 0;
            foreach(IXLRow row in rows) {
                if(j < (rowsCount - 1)) {
                    tableSectionData.InsertRow(j + 1);
                }
                tableSectionData.SetRowHeight(j, _lengthConverter.ConvertExcelRowHeightToInternal(row.Height));
                j++;
            }

            IXLRanges mergedRanges = worksheet.MergedRanges;
            foreach(IXLRange mergedRange in mergedRanges) {
                IXLAddress leftTopCell = mergedRange.FirstCell().Address;
                IXLAddress bottomRightCell = mergedRange.LastCell().Address;
                // в TableSectionData индексация с 0, в Excel индексация с 1
                tableSectionData.MergeCells(new TableMergedCell(
                    leftTopCell.RowNumber - 1,
                    leftTopCell.ColumnNumber - 1,
                    bottomRightCell.RowNumber - 1,
                    bottomRightCell.ColumnNumber - 1));
            }
            return tableSectionData;
        }

        private void RefreshIfNeed(TableSectionData tableSectionData) {
            if(tableSectionData.NeedsRefresh) {
                tableSectionData.RefreshData();
            }
            _revitRepository.Document.Regenerate(); // если не вызвать, размер шрифта на листе может не обновиться
        }

        private TableSectionData FillTable(TableSectionData tableSectionData, IXLWorksheet worksheet) {
            for(var i = 0; i < tableSectionData.LastRowNumber + 1; i++) {
                for(var j = 0; j < tableSectionData.LastColumnNumber + 1; j++) {
                    IXLCell cell = worksheet.Cell(i + 1, j + 1);
                    tableSectionData.SetCellText(i, j, GetCellValue(cell));
                    TableCellStyle style = GetTableCellStyle(cell);
                    tableSectionData.SetCellStyle(i, j, style);
                }
            }
            return tableSectionData;
        }

        private TableCellStyle GetTableCellStyle(IXLCell cell) {
            var hideTopBorder = HideCellBorder(cell, CellBorder.Top, true);
            var hideBottomBorder = HideCellBorder(cell, CellBorder.Bottom, true);
            var hideLeftBorder = HideCellBorder(cell, CellBorder.Left, true);
            var hideRightBorder = HideCellBorder(cell, CellBorder.Right, true);

            var style = new TableCellStyle() {
                FontVerticalAlignment = GetVerticalAlignmentStyle(cell.Style.Alignment.Vertical),
                FontHorizontalAlignment = GetHorizontalAlignmentStyle(cell.Style.Alignment.Horizontal),
                BackgroundColor = GetColor(cell.Worksheet.Workbook, cell.Style.Fill.BackgroundColor),
                BorderTopLineStyle = ElementId.InvalidElementId,
                BorderBottomLineStyle = ElementId.InvalidElementId,
                BorderLeftLineStyle = ElementId.InvalidElementId,
                BorderRightLineStyle = ElementId.InvalidElementId,
                TextSize = _lengthConverter.ConvertExcelFontSizeToInternal(cell.Style.Font.FontSize),
                IsFontItalic = cell.Style.Font.Italic,
                IsFontBold = cell.Style.Font.Bold,
                IsFontUnderline = cell.Style.Font.Underline != XLFontUnderlineValues.None,
                TextColor = GetColor(cell.Worksheet.Workbook, cell.Style.Font.FontColor),
                FontName = cell.Style.Font.FontName
            };
            style.SetCellStyleOverrideOptions(new TableCellStyleOverrideOptions() {
                VerticalAlignment = true,
                HorizontalAlignment = true,
                BackgroundColor = true,
                BorderTopLineStyle = hideTopBorder,
                BorderBottomLineStyle = hideBottomBorder,
                BorderLeftLineStyle = hideLeftBorder,
                BorderRightLineStyle = hideRightBorder,
                FontSize = true,
                Italics = true,
                Bold = true,
                Underline = true,
                FontColor = true,
                Font = true
            });
            return style;
        }

        /// <summary>
        /// Пытается получить соседнюю ячейку с заданной стороны
        /// </summary>
        /// <param name="cell">Текущая ячейка</param>
        /// <param name="border">Сторона текущей ячейки, к которой примыкает соседняя ячейка</param>
        /// <param name="neighboringCell">Соседняя ячейка, если она есть</param>
        /// <returns>True, если соседняя ячейка найдена, иначе False</returns>
        private bool TryGetNeighboringCell(IXLCell cell, CellBorder border, out IXLCell neighboringCell) {
            try {
                switch(border) {
                    case CellBorder.Left:
                        neighboringCell = cell.Worksheet.Cell(cell.Address.RowNumber, cell.Address.ColumnNumber - 1);
                        return true;
                    case CellBorder.Top:
                        neighboringCell = cell.Worksheet.Cell(cell.Address.RowNumber - 1, cell.Address.ColumnNumber);
                        return true;
                    case CellBorder.Right:
                        neighboringCell = cell.Worksheet.Cell(cell.Address.RowNumber, cell.Address.ColumnNumber + 1);
                        return true;
                    case CellBorder.Bottom:
                        neighboringCell = cell.Worksheet.Cell(cell.Address.RowNumber + 1, cell.Address.ColumnNumber);
                        return true;
                    default:
                        neighboringCell = null;
                        return false;
                }
            } catch(ArgumentOutOfRangeException) {
                neighboringCell = null;
                return false;
            }
        }

        /// <summary>
        /// Проверяет, надо ли скрывать границу ячейки с заданной стороны с учетом соседней ячейки с этой стороны
        /// </summary>
        /// <param name="cell">Ячейка, у которой проверяется скрытие границы</param>
        /// <param name="cellBorder">Сторона ячейки</param>
        /// <param name="considerNeighbor">Учитывать ли соседнюю ячейку</param>
        /// <returns>True, если у заданной ячейки надо скрыть заданную сторону границы, иначе False</returns>
        private bool HideCellBorder(IXLCell cell, CellBorder cellBorder, bool considerNeighbor) {
            bool hideCurrentCellBorder = HideCellBorder(cell, cellBorder);
            if(considerNeighbor) {
                var neighboringCellExist = TryGetNeighboringCell(cell, cellBorder, out IXLCell neighboringCell);
                return hideCurrentCellBorder
                    && (neighboringCellExist && HideCellBorder(neighboringCell, GetInvertedCellBorder(cellBorder))
                        || !neighboringCellExist);
            } else {
                return hideCurrentCellBorder;
            }
        }

        private CellBorder GetInvertedCellBorder(CellBorder cellBorder) {
            switch(cellBorder) {
                case CellBorder.Left:
                    return CellBorder.Right;
                case CellBorder.Top:
                    return CellBorder.Bottom;
                case CellBorder.Right:
                    return CellBorder.Left;
                case CellBorder.Bottom:
                    return CellBorder.Top;
                default:
                    throw new InvalidOperationException($"Нельзя инвертировать границу {cellBorder}");
            }
        }

        /// <summary>
        /// Проверяет, надо ли скрывать границу ячейки с заданной стороны
        /// </summary>
        /// <param name="cell">Ячейка, у которой проверяется скрытие границы</param>
        /// <param name="cellBorder">Сторона ячейки</param>
        /// <returns>True, если у заданной ячейки надо скрыть заданную сторону границы, иначе False</returns>
        private bool HideCellBorder(IXLCell cell, CellBorder cellBorder) {
            var border = cell.Style.Border;
            switch(cellBorder) {
                case CellBorder.Left:
                    return CellBorderColorIsWhite(cell.Worksheet.Workbook, border.LeftBorderColor)
                        || border.LeftBorder == XLBorderStyleValues.None;
                case CellBorder.Top:
                    return CellBorderColorIsWhite(cell.Worksheet.Workbook, border.TopBorderColor)
                        || border.TopBorder == XLBorderStyleValues.None;
                case CellBorder.Right:
                    return CellBorderColorIsWhite(cell.Worksheet.Workbook, border.RightBorderColor)
                        || border.RightBorder == XLBorderStyleValues.None;
                case CellBorder.Bottom:
                    return CellBorderColorIsWhite(cell.Worksheet.Workbook, border.BottomBorderColor)
                        || border.BottomBorder == XLBorderStyleValues.None;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Проверяет, является ли цвет границы ячейки белым
        /// </summary>
        /// <param name="workbook">Книга Excel</param>
        /// <param name="xlColor">Цвет границы ячейки Excel</param>
        /// <returns>True, если цвет границы ячейки - белый, иначе False</returns>
        private bool CellBorderColorIsWhite(IXLWorkbook workbook, XLColor xlColor) {
            var color = GetBorderColor(workbook, xlColor);
            return color.Red == 255
                && color.Green == 255
                && color.Blue == 255;
        }

        /// <summary>
        /// Конвертирует цвет Excel в цвет Revit
        /// </summary>
        /// <param name="workbook">Книга Excel</param>
        /// <param name="xlColor">Цвет Excel</param>
        /// <returns>Цвет Revit</returns>
        private Color GetColor(IXLWorkbook workbook, XLColor xlColor) {
            var colorType = xlColor.ColorType;
            switch(colorType) {
                case XLColorType.Theme:
                    var themeColor = xlColor.ThemeColor;
                    if(!_colorsDictionary.ContainsKey(themeColor)) {
                        System.Drawing.Color rgbColor = workbook.Theme.ResolveThemeColor(themeColor).Color;
                        _colorsDictionary.Add(themeColor, rgbColor);
                    }
                    System.Drawing.Color color = _colorsDictionary[themeColor];
                    return new Color(color.R, color.G, color.B);
                default:
                    return new Color(xlColor.Color.R, xlColor.Color.G, xlColor.Color.B);
            }
        }

        /// <summary>
        /// Конвертирует цвет границы Excel в цвет Revit
        /// </summary>
        /// <param name="workbook">Книга Excel</param>
        /// <param name="xlColor">Цвет границы ячейки Excel</param>
        /// <returns>Цвет Revit</returns>
        private Color GetBorderColor(IXLWorkbook workbook, XLColor xlColor) {
            var colorType = xlColor.ColorType;
            if(colorType == XLColorType.Indexed
                        && xlColor.Indexed == 64
                        && xlColor.Color.R == 255
                        && xlColor.Color.G == 255
                        && xlColor.Color.B == 255) {
                // здесь цвет границы "по умолчанию", который отображается в Excel как черный, а в api как белый
                return new Color(0, 0, 0); // Black
            } else {
                return GetColor(workbook, xlColor);
            }
        }

        private VerticalAlignmentStyle GetVerticalAlignmentStyle(XLAlignmentVerticalValues xlAlignment) {
            switch(xlAlignment) {
                case XLAlignmentVerticalValues.Bottom:
                    return VerticalAlignmentStyle.Bottom;
                case XLAlignmentVerticalValues.Top:
                    return VerticalAlignmentStyle.Top;
                default:
                    return VerticalAlignmentStyle.Middle;
            }
        }

        private HorizontalAlignmentStyle GetHorizontalAlignmentStyle(XLAlignmentHorizontalValues xlAlignment) {
            switch(xlAlignment) {
                case XLAlignmentHorizontalValues.Left:
                    return HorizontalAlignmentStyle.Left;
                case XLAlignmentHorizontalValues.Right:
                    return HorizontalAlignmentStyle.Right;
                default:
                    return HorizontalAlignmentStyle.Center;
            }
        }

        private string GetCellValue(IXLCell cell) {
            var cellValue = cell.Value;
            var cellType = cellValue.Type;
            switch(cellType) {
                case XLDataType.Boolean:
                    return cellValue.GetBoolean().ToString();
                case XLDataType.Number:
                    double dValue = cellValue.GetNumber();
                    return dValue % 1 == 0
                        ? ((int) dValue).ToString()
                        : dValue.ToString(CultureInfo.CurrentCulture);
                case XLDataType.Text:
                    return cellValue.GetText();
                case XLDataType.DateTime:
                    return cellValue.GetDateTime().ToString(CultureInfo.CurrentCulture);
                case XLDataType.TimeSpan:
                    return cellValue.GetTimeSpan().ToString("c", CultureInfo.CurrentCulture);
                default:
                    return string.Empty;
            }
        }
    }
}
