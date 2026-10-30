using System.Globalization;
using System.Text.RegularExpressions;

using RevitVORChecker.Core;

namespace RevitVORChecker.Models.Excel;

/// <summary>
/// Нормализация текста и чисел ячеек при сверке выгрузок ВОР.
/// </summary>
internal static class ExcelText {
    /// <summary>
    /// Единицы измерения, по которым находится столбец единиц в таблицах прямого расчёта.
    /// </summary>
    public static readonly HashSet<string> Units = new() {
        "м", "м²", "м³", "шт.", "шт", "кг", "м.п.", "т", "м2", "м3", "п.м.", "пм"
    };

    /// <summary>
    /// Текст ячейки без переводов строк и лишних пробелов.
    /// </summary>
    public static string Normalize(string? text) {
        string singleLine = (text ?? "").Replace("\r", " ").Replace("\n", " ");
        return Regex.Replace(singleLine, @"\s+", " ").Trim();
    }

    /// <summary>
    /// Число из текста ячейки (запятая или точка, пробелы-разделители разрядов) или null.
    /// </summary>
    public static double? ParseNumber(string? text) {
        string compact = (text ?? "").Trim().Replace(" ", "").Replace(" ", "");
        if(compact.Length == 0 || !Regex.IsMatch(compact, @"^-?\d+([.,]\d+)?([eE]-?\d+)?$")) {
            return null;
        }

        return double.TryParse(compact.Replace(",", "."), NumberStyles.Float, PythonFormat.Invariant, out double value)
            ? value
            : null;
    }

    /// <summary>
    /// Число знаков после запятой в записи числа.
    /// </summary>
    public static int CountDecimals(string? text) {
        // пробелы-разделители разрядов («1 234,56») убираются, как в ParseNumber
        string compact = PythonFormat.Trim(text).Replace(" ", "").Replace(" ", "");
        Match match = Regex.Match(compact, @"^-?\d+[.,](\d+)$");
        return match.Success ? match.Groups[1].Value.Length : 0;
    }

    public static bool IsDigits(string text) {
        return text.Length > 0 && text.All(char.IsDigit);
    }

    /// <summary>
    /// Этажи из значения столбца «Этаж»: «02-04 этаж» → 02, 03, 04; «П01, 01 этаж» → П01, 01.
    /// </summary>
    public static HashSet<string> ParseFloors(string? value) {
        var floors = new HashSet<string>();
        foreach(string part in Regex.Split((value ?? "").Replace("этаж", " "), @"[,()\s]+")) {
            Match range = Regex.Match(part, @"^(К|П|ТХ|Т|Д)?(\d{2})-(К|П|ТХ|Т|Д)?(\d{2})$");
            if(range.Success) {
                int first = int.Parse(range.Groups[2].Value, CultureInfo.InvariantCulture);
                int last = int.Parse(range.Groups[4].Value, CultureInfo.InvariantCulture);
                for(int number = first; number <= last; number++) {
                    floors.Add(range.Groups[1].Value + number.ToString("00"));
                }

                continue;
            }

            Match single = Regex.Match(part, @"^(К|П|ТХ|Т|Д)?(\d{2})$");
            if(single.Success) {
                floors.Add(single.Groups[1].Value + single.Groups[2].Value);
            }
        }

        return floors;
    }
}
