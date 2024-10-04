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
        private readonly double _footInMm;
        private readonly Dictionary<XLThemeColor, System.Drawing.Color> _colorsDictionary;

        public ScheduleImporter(
            RevitRepository revitRepository,
            LengthConverter lengthConverter,
            ExcelReader excelReader) {
            _revitRepository = revitRepository ?? throw new System.ArgumentNullException(nameof(revitRepository));
            _lengthConverter = lengthConverter ?? throw new System.ArgumentNullException(nameof(lengthConverter));
            _excelReader = excelReader ?? throw new System.ArgumentNullException(nameof(excelReader));

            _footInMm = _lengthConverter.ConvertFromInternal(1);
            _colorsDictionary = new Dictionary<XLThemeColor, System.Drawing.Color>();
        }


        public void ImportSchedule(string path, out string[] failedWorksheets) {
            if(!File.Exists(path)) {
                throw new FileNotFoundException("Excel файл не найден.", path);
            }

            // В заголовке спеки в 2022 и 2024 ревите можно:
            // добавлять, удалять, объединять, изменять размер столбцов и строк

            // устанавливать в качестве стиля линии для границы ячейки стиль линий из проекта +

            // устанавливать шрифт в ячейке +
            //               размер шрифта +
            //               начертание (полужирный +, курсив +, подчеркнутый +)
            //               цвет шрифта +

            // устанавливать выравнивание текста в ячейке по горизонтали, вертикали +

            // заливку цветом ячейки (тонирование) +
            using(var excelData = _excelReader.ReadExcel(path)) {
                using(Transaction trans = _revitRepository.Document.StartTransaction("Импорт Excel")) {
                    List<string> failedSheets = new List<string>();
                    foreach(var worksheet in excelData.Workbook.Worksheets) {
                        try {
                            string scheduleName = CreateScheduleName(path, worksheet.Name);
                            var schedule = _revitRepository.CreateSchedule(scheduleName);
                            WriteData(schedule, worksheet);
                        } catch(Autodesk.Revit.Exceptions.ApplicationException) {
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

        private TableSectionData CreateTableSectionData(TableData tableData, IXLWorksheet worksheet) {
            IXLColumns columns = GetColumns(worksheet);
            IXLRows rows = GetRows(worksheet);

            var defaultFontSize = columns.Style.Font.FontSize;
            var tableSectionData = tableData.GetSectionData(SectionType.Header);
            tableData.Width = columns.Sum(col => col.Width) / _footInMm;
            int columnsCount = columns.Count();
            int i = 0;
            foreach(var col in columns) {
                if(i < (columnsCount - 1)) {
                    tableSectionData.InsertColumn(i + 1);
                }
                // fucking width
                // https://github.com/ClosedXML/ClosedXML/wiki/Cell-Dimensions#width-1
                tableSectionData.SetColumnWidth(i, col.Width / _footInMm);
                i++;
            }

            int rowsCount = rows.Count();
            int j = 0;
            foreach(var row in rows) {
                if(j < (rowsCount - 1)) {
                    tableSectionData.InsertRow(j + 1);
                }
                tableSectionData.SetRowHeight(j, row.Height / (_footInMm * 7));
                j++;
            }

            var mergedRanges = worksheet.MergedRanges;
            foreach(var mergedRange in mergedRanges) {
                var leftTopCell = mergedRange.FirstCell().Address;
                var bottomRightCell = mergedRange.LastCell().Address;
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
            var cellStyle = cell.Style;


            var cellColor = cell.Style.Fill.BackgroundColor;

            var alignment = cell.Style.Alignment; // +
            var vAlignment = alignment.Vertical; // +
            var hAlignment = alignment.Horizontal; // +


            var border = cell.Style.Border;
            var topBorder = border.TopBorder;
            var bottomBorder = border.BottomBorder;
            var leftBorder = border.LeftBorder;
            var rightBorder = border.RightBorder;


            var font = cell.Style.Font;
            var fontSize = font.FontSize; // +
            var fontName = font.FontName; // +
            bool fontIsItalic = font.Italic; // +
            bool fontIsBold = font.Bold; // +
            bool fontIsUnderline = font.Underline != XLFontUnderlineValues.None; // +
            var fontColor = font.FontColor; // +

            var style = new TableCellStyle() {
                FontVerticalAlignment = GetVerticalAlignmentStyle(alignment.Vertical),
                FontHorizontalAlignment = GetHorizontalAlignmentStyle(alignment.Horizontal),

                TextSize = fontSize,// GetFontSizeInInternal(fontSize),
                IsFontItalic = font.Italic,
                IsFontBold = font.Bold,
                IsFontUnderline = font.Underline != XLFontUnderlineValues.None,

                TextColor = GetColor(cell.Worksheet.Workbook, fontColor),

                FontName = fontName
            };
            style.SetCellStyleOverrideOptions(new TableCellStyleOverrideOptions() {
                VerticalAlignment = true,
                HorizontalAlignment = true,

                FontSize = true,
                Italics = true,
                Bold = true,
                Underline = true,

                FontColor = true,

                Font = true
            });
            return style;
        }

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
                    return cellValue.GetDateTime().ToString();
                case XLDataType.TimeSpan:
                    return cellValue.GetTimeSpan().ToString();
                default:
                    return string.Empty;
            }
        }
    }
}
