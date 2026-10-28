using System.Text.RegularExpressions;
using System.Xml.Linq;

using RevitVORChecker.Core;

namespace RevitVORChecker.Models.Excel;

/// <summary>
/// Лист «Сводная ВОР»: сводная таблица обновлена и её итог равен сумме листа «Исходная ВОР».
/// </summary>
internal static class PivotTableChecker {
    public static void Check(
        XlsxReader workbook,
        OrderedMap<string, string> sheets,
        int sourceRowCount,
        double sourceTotal,
        string fileName,
        FindingWriter writer) {

        string pivotSheet = sheets.Keys.FirstOrDefault(name => name.ToLowerInvariant().Contains("сводн"));
        if(pivotSheet == null) {
            writer.Add(fileName + ": нет листа «Сводная ВОР».");
            return;
        }

        CheckPivotCaches(workbook, sourceRowCount, fileName, writer);

        List<SheetRow> rows = workbook.ReadSheet(sheets[pivotSheet]);
        int? totalColumn = null;
        foreach(SheetRow row in rows.Take(15)) {
            foreach(var cell in row.Cells) {
                if(ExcelText.Normalize(cell.Value.Text) == "Общий итог") {
                    totalColumn = cell.Key;
                }
            }
        }

        double? grandTotal = null;
        foreach(SheetRow row in rows) {
            var firstCell = row.Cells.OrderBy(cell => cell.Key).Take(1).Select(cell => ExcelText.Normalize(cell.Value.Text)).ToList();
            if(firstCell.Count > 0
               && firstCell[0] == "Общий итог"
               && totalColumn != null
               && row.Cells.ContainsKey(totalColumn.Value)) {
                grandTotal = ExcelText.ParseNumber(row.Cells[totalColumn.Value].Text);
            }
        }

        if(grandTotal != null && Math.Abs(grandTotal.Value - sourceTotal) > 0.01) {
            writer.Add(
                fileName + ": итог сводной таблицы (" + PythonFormat.GeneralFormat(grandTotal.Value)
                + ") не равен сумме «Количество» листа «Исходная ВОР» ("
                + PythonFormat.GeneralFormat(PythonFormat.Round(sourceTotal, 3))
                + ") — сводная не обновлена или источник неполный.");
        }
    }

    private static void CheckPivotCaches(XlsxReader workbook, int sourceRowCount, string fileName, FindingWriter writer) {
        var cacheParts = workbook.GetEntryNames()
            .Where(name => Regex.IsMatch(name, @"^xl/pivotCache/pivotCacheDefinition\d+\.xml$"));
        foreach(string cachePart in cacheParts) {
            XElement cache = workbook.ReadPart(cachePart).Root;
            string recordCount = (string) cache.Attribute("recordCount");
            XElement source = cache.Element(XlsxReader.SpreadsheetNamespace + "cacheSource")
                ?.Element(XlsxReader.SpreadsheetNamespace + "worksheetSource");
            if(recordCount != null && int.Parse(recordCount) != sourceRowCount) {
                writer.Add(
                    fileName + ": сводная таблица не обновлена — в кэше " + recordCount
                    + " строк, на листе «Исходная ВОР» " + sourceRowCount
                    + " строк данных (ПКМ по сводной → «Обновить»).");
            }

            if(source != null && !string.IsNullOrEmpty((string) source.Attribute("ref"))) {
                writer.Add(
                    fileName + ": источник сводной таблицы — " + ((string) source.Attribute("sheet") ?? "?") + "!"
                    + (string) source.Attribute("ref") + "; проверить, что диапазон охватывает все строки.",
                    Findings.FindingKinds.Info);
            }
        }
    }
}
