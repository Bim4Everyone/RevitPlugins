using System.Globalization;
using System.IO.Compression;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace RevitVORChecker.Core;

/// <summary>
/// Чтение книг .xlsx/.xlsm без сторонних библиотек (архив ZIP + XML).
/// </summary>
internal class XlsxReader : IDisposable {
    public static readonly XNamespace SpreadsheetNamespace =
        "http://schemas.openxmlformats.org/spreadsheetml/2006/main";

    public static readonly XNamespace RelationshipsNamespace =
        "http://schemas.openxmlformats.org/officeDocument/2006/relationships";

    private readonly ZipArchive _archive;
    private readonly List<string> _sharedStrings = new();

    public XlsxReader(string path) {
        // файл может быть открыт в Excel — открываем с разрешением совместного доступа
        var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        try {
            _archive = new ZipArchive(stream, ZipArchiveMode.Read);
        } catch(Exception) {
            // не архив ZIP (книга с паролем, старый .xls, повреждённый файл) — файл не остаётся занятым
            stream.Dispose();
            throw;
        }

        XDocument? sharedStrings = ReadPart("xl/sharedStrings.xml");
        if(sharedStrings != null) {
            foreach(XElement item in sharedStrings.Root.Elements(SpreadsheetNamespace + "si")) {
                _sharedStrings.Add(string.Concat(item.Descendants(SpreadsheetNamespace + "t").Select(t => t.Value)));
            }
        }
    }

    /// <summary>
    /// Обязательная часть книги.
    /// </summary>
    /// <exception cref="InvalidDataException">Части нет — файл не книга Excel.</exception>
    public XDocument ReadRequiredPart(string name) {
        return ReadPart(name) ?? throw new InvalidDataException("No part " + name);
    }

    public XDocument? ReadPart(string name) {
        ZipArchiveEntry entry = _archive.GetEntry(name);
        if(entry == null) {
            return null;
        }

        using Stream stream = entry.Open();
        return XDocument.Load(stream);
    }

    /// <summary>
    /// Имена записей архива в порядке центрального каталога.
    /// </summary>
    public IEnumerable<string> GetEntryNames() {
        return _archive.Entries.Select(entry => entry.FullName);
    }

    /// <summary>
    /// Имена листов в порядке книги.
    /// </summary>
    public List<string> GetSheetNames() {
        return ReadRequiredPart("xl/workbook.xml")
            .Descendants(SpreadsheetNamespace + "sheet")
            .Select(sheet => (string) sheet.Attribute("name"))
            .ToList();
    }

    /// <summary>
    /// Листы книги (имя → путь части архива) в порядке книги.
    /// </summary>
    public OrderedMap<string, string> GetSheetPaths() {
        var targets = ReadRequiredPart("xl/_rels/workbook.xml.rels").Root.Elements()
            .ToDictionary(relation => (string) relation.Attribute("Id"), relation => (string) relation.Attribute("Target"));
        var sheets = new OrderedMap<string, string>();
        foreach(XElement sheet in ReadRequiredPart("xl/workbook.xml").Descendants(SpreadsheetNamespace + "sheet")) {
            string target = targets[(string) sheet.Attribute(RelationshipsNamespace + "id")].TrimStart('/');
            if(!target.StartsWith("xl/")) {
                target = "xl/" + target;
            }

            sheets[(string) sheet.Attribute("name")] = target;
        }

        return sheets;
    }

    /// <summary>
    /// Значение ячейки как строка: общие строки, inlineStr, числа — как записаны в XML.
    /// </summary>
    public string? GetCellText(XElement cell) {
        string cellType = (string) cell.Attribute("t");
        if(cellType == "inlineStr") {
            return string.Concat(cell.Descendants(SpreadsheetNamespace + "t").Select(t => t.Value));
        }

        XElement value = cell.Element(SpreadsheetNamespace + "v");
        if(value == null) {
            return null;
        }

        return cellType == "s" ? _sharedStrings[int.Parse(value.Value, CultureInfo.InvariantCulture)] : value.Value;
    }

    /// <summary>
    /// Строки листа с ячейками (номер столбца → значение и вид значения).
    /// </summary>
    public List<SheetRow> ReadSheet(string partPath) {
        var rows = new List<SheetRow>();
        int previousRowNumber = 0;
        foreach(XElement rowElement in ReadRequiredPart(partPath).Descendants(SpreadsheetNamespace + "row")) {
            // ссылки r необязательны: без них строка и ячейка идут следом за предыдущими
            string rowReference = (string) rowElement.Attribute("r");
            int rowNumber = rowReference != null ? int.Parse(rowReference, CultureInfo.InvariantCulture) : previousRowNumber + 1;
            previousRowNumber = rowNumber;
            var row = new SheetRow(rowNumber);
            int previousColumnIndex = -1;
            foreach(XElement cell in rowElement.Elements(SpreadsheetNamespace + "c")) {
                string cellReference = (string) cell.Attribute("r");
                int columnIndex = cellReference != null ? GetColumnIndex(cellReference) : previousColumnIndex + 1;
                previousColumnIndex = columnIndex;
                SheetCell? sheetCell = ReadCell(cell);
                if(sheetCell != null) {
                    row.Cells[columnIndex] = sheetCell;
                }
            }

            rows.Add(row);
        }

        return rows;
    }

    public void Dispose() {
        _archive.Dispose();
    }

    private SheetCell? ReadCell(XElement cell) {
        XElement value = cell.Element(SpreadsheetNamespace + "v");
        string cellType = (string) cell.Attribute("t");
        string text;
        bool isText;
        if(cellType == "inlineStr") {
            text = string.Concat(cell.Descendants(SpreadsheetNamespace + "t").Select(t => t.Value));
            isText = true;
        } else if(value == null) {
            return null;
        } else if(cellType == "s") {
            text = _sharedStrings[int.Parse(value.Value, CultureInfo.InvariantCulture)];
            isText = true;
        } else {
            text = value.Value ?? "";
            isText = cellType == "str";
        }

        bool hasFormula = cell.Element(SpreadsheetNamespace + "f") != null;
        return new SheetCell(text, isText, hasFormula);
    }

    private static int GetColumnIndex(string cellReference) {
        string letters = Regex.Match(cellReference, "[A-Z]+").Value;
        int index = 0;
        foreach(char letter in letters) {
            index = index * 26 + letter - 64;
        }

        return index - 1;
    }
}

/// <summary>
/// Ячейка листа Excel.
/// </summary>
internal class SheetCell {
    public SheetCell(string text, bool isText, bool hasFormula) {
        Text = text;
        IsText = isText;
        HasFormula = hasFormula;
    }

    public string Text { get; }

    /// <summary>
    /// Значение записано текстом (а не числом).
    /// </summary>
    public bool IsText { get; }

    public bool HasFormula { get; }
}

/// <summary>
/// Строка листа Excel.
/// </summary>
internal class SheetRow {
    public SheetRow(int rowNumber) {
        RowNumber = rowNumber;
    }

    public int RowNumber { get; }

    /// <summary>
    /// Ячейки строки: индекс столбца (с нуля) → ячейка, в порядке следования в файле.
    /// </summary>
    public OrderedMap<int, SheetCell> Cells { get; } = new();
}
