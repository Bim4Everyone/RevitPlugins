using System.Text.RegularExpressions;
using System.Xml.Linq;

using RevitVORChecker.Core;

namespace RevitVORChecker.Models.Checks;

/// <summary>
/// Классификатор видов работ (.xlsx): код работы → наименование и единица измерения; коды без единицы — главы.
/// </summary>
internal class WorkClassifier {
    public WorkClassifier(string path) {
        using var reader = new XlsxReader(path);
        List<string> sheetNames = reader.GetSheetNames();
        for(int sheetNumber = 1; sheetNumber <= sheetNames.Count; sheetNumber++) {
            if(sheetNames[sheetNumber - 1].ToUpperInvariant().Contains("НЕ ИСПОЛЬЗ")) {
                continue;
            }

            XDocument sheet = reader.ReadPart("xl/worksheets/sheet" + sheetNumber + ".xml");
            if(sheet == null) {
                continue;
            }

            foreach(XElement row in sheet.Descendants(XlsxReader.SpreadsheetNamespace + "row")) {
                ReadRow(reader, row);
            }
        }
    }

    /// <summary>
    /// Работы: код → наименование и единица измерения.
    /// </summary>
    public Dictionary<string, ClassifierWork> Works { get; } = new();

    /// <summary>
    /// Главы: код → наименование главы.
    /// </summary>
    public Dictionary<string, string> Chapters { get; } = new();

    /// <summary>
    /// Единица измерения в едином написании: «м3» → «м³», «шт» → «шт.».
    /// </summary>
    public static string NormalizeUnit(string unit) {
        string normalized = (unit ?? "").Trim()
            .Replace("м3", "м³")
            .Replace("м2", "м²")
            .Replace("куб.м", "м³")
            .Replace("кв.м", "м²");
        return normalized == "шт" || normalized == "шт." ? "шт." : normalized;
    }

    private void ReadRow(XlsxReader reader, XElement row) {
        var values = row.Elements(XlsxReader.SpreadsheetNamespace + "c")
            .Select(cell => reader.GetCellText(cell) ?? "")
            .ToList();
        if(values.Count == 0 || !Regex.IsMatch(values[0], @"^г\d")) {
            return;
        }

        string code = values[0].Trim();
        string name = (values.Count > 1 ? values[1] : "").Trim();
        string unit = (values.Count > 2 ? values[2] : "").Trim();
        if(unit.Length > 0) {
            Works[code] = new ClassifierWork(name, NormalizeUnit(unit));
        } else {
            Chapters[code] = name;
        }
    }
}

/// <summary>
/// Работа Классификатора.
/// </summary>
internal class ClassifierWork {
    public ClassifierWork(string name, string unit) {
        Name = name;
        Unit = unit;
    }

    public string Name { get; }

    public string Unit { get; }
}
