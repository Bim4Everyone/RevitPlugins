using System.Text.RegularExpressions;
using System.Xml.Linq;

using RevitVORChecker.Core;
using RevitVORChecker.Models.Findings;

namespace RevitVORChecker.Models.Excel;

/// <summary>
/// Лист «Сводная ВОР»: сводная таблица обновлена и её итог равен сумме листа «Исходная ВОР».
/// </summary>
internal static class PivotTableChecker {
    public static void Check(
        XlsxReader workbook,
        OrderedMap<string, string> sheets,
        string sourceSheet,
        int sourceRowCount,
        double sourceTotal,
        string fileName,
        FindingWriter writer) {

        string pivotSheet = sheets.Keys.FirstOrDefault(name => name.ToLowerInvariant().Contains("сводн"));
        if(pivotSheet == null) {
            writer.Add(fileName + ": нет листа «Сводная ВОР».");
            return;
        }

        CheckPivotCaches(workbook, sourceSheet, sourceRowCount, fileName, writer);

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
            var orderedCells = row.Cells.OrderBy(cell => cell.Key).ToList();
            if(orderedCells.Count == 0 || ExcelText.Normalize(orderedCells[0].Value.Text) != "Общий итог") {
                continue;
            }

            if(totalColumn != null && row.Cells.ContainsKey(totalColumn.Value)) {
                grandTotal = ExcelText.ParseNumber(row.Cells[totalColumn.Value].Text);
            } else {
                // у сводной без полей в столбцах нет заголовка «Общий итог» — итог в последней числовой ячейке строки
                grandTotal = orderedCells
                    .Select(cell => ExcelText.ParseNumber(cell.Value.Text))
                    .LastOrDefault(number => number != null);
            }
        }

        if(grandTotal == null) {
            writer.Add(
                fileName + ": на листе «" + pivotSheet + "» нет строки «Общий итог» (общие итоги сводной таблицы "
                + "отключены) — сверка итога сводной с листом «Исходная ВОР» не выполнена. Включить общие итоги "
                + "или сверить итог вручную.",
                FindingKinds.Clarify);
            return;
        }

        if(Math.Abs(grandTotal.Value - sourceTotal) > 0.01) {
            writer.Add(
                fileName + ": итог сводной таблицы (" + PythonFormat.GeneralFormat(grandTotal.Value)
                + ") не равен сумме «Количество» листа «Исходная ВОР» ("
                + PythonFormat.GeneralFormat(PythonFormat.Round(sourceTotal, 3))
                + ") — сводная не обновлена или источник неполный.");
        }
    }

    private static void CheckPivotCaches(
        XlsxReader workbook,
        string sourceSheet,
        int sourceRowCount,
        string fileName,
        FindingWriter writer) {

        var cacheParts = workbook.GetEntryNames()
            .Where(name => Regex.IsMatch(name, @"^xl/pivotCache/pivotCacheDefinition\d+\.xml$"));
        foreach(string cachePart in cacheParts) {
            XElement cache = workbook.ReadRequiredPart(cachePart).Root;
            string recordCount = (string) cache.Attribute("recordCount");
            XElement? source = cache.Element(XlsxReader.SpreadsheetNamespace + "cacheSource")
                ?.Element(XlsxReader.SpreadsheetNamespace + "worksheetSource");

            // сводные по другим листам книги не сверяются с «Исходной ВОР»; источник-таблица (без листа) — сверяется
            string cacheSheet = (string) source?.Attribute("sheet");
            if(cacheSheet != null && !string.Equals(cacheSheet.Trim(), sourceSheet.Trim(), StringComparison.OrdinalIgnoreCase)) {
                continue;
            }

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
                    FindingKinds.Info);
            }
        }
    }
}
