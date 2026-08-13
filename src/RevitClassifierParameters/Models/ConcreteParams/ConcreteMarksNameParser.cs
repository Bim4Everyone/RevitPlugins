using System.Globalization;
using System.Text.RegularExpressions;

namespace RevitClassifierParameters.Models.ConcreteParams;

/// <summary>
/// Разбирает имя типоразмера железобетонного элемента и извлекает марки бетона B, F, W.
/// Логика повторяет исходный Python-плагин "Параметры бетона".
/// Пример имени: "НН_Перекрытие-240 (ЖБ B30 F150 W6)" или "Ф_Подготовка-70 (Б B7.5)".
/// </summary>
internal class ConcreteMarksNameParser {
    // Буква "B" может быть написана на латинице или на кириллице ("В").
    private static readonly Regex _markBRegex =
        new(@"[BВ]([0-9]+(?:[.,][0-9]+)?)", RegexOptions.Compiled);
    private static readonly Regex _markFRegex =
        new(@"F([0-9]+(?:[.,][0-9]+)?)", RegexOptions.Compiled);
    private static readonly Regex _markWRegex =
        new(@"W([0-9]+(?:[.,][0-9]+)?)", RegexOptions.Compiled);

    // Материальная часть имени в круглых скобках, например "(ЖБ B30 F150 W6)".
    private static readonly Regex _materialPartRegex =
        new(@"\(.*\)", RegexOptions.Compiled);

    /// <summary>
    /// Разбирает имя типоразмера и возвращает извлечённые марки бетона.
    /// Значения по умолчанию: B = 0, F = 0, W = 0, тип материала "B0".
    /// </summary>
    public ConcreteMarks Parse(string typeName) {
        if(string.IsNullOrWhiteSpace(typeName)) {
            return new ConcreteMarks(0.0, 0.0, 0.0, "B0");
        }

        double markB = 0.0;
        double markF = 0.0;
        double markW = 0.0;
        string materialType = "B0";

        // Марку B ищем только в материальной части имени (в скобках).
        var materialPartMatch = _materialPartRegex.Match(typeName);
        if(materialPartMatch.Success) {
            var matchB = _markBRegex.Match(materialPartMatch.Value);
            if(matchB.Success) {
                string valueB = matchB.Groups[1].Value.Replace(",", ".");
                materialType = "B" + valueB;
                markB = ToDouble(valueB);
            }
        }

        // Марки F и W ищем во всём имени, как в исходном плагине.
        var matchF = _markFRegex.Match(typeName);
        if(matchF.Success) {
            markF = ToDouble(matchF.Groups[1].Value.Replace(",", "."));
        }

        var matchW = _markWRegex.Match(typeName);
        if(matchW.Success) {
            markW = ToDouble(matchW.Groups[1].Value.Replace(",", "."));
        }

        return new ConcreteMarks(markB, markF, markW, materialType);
    }

    private static double ToDouble(string value) {
        return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double result)
            ? result
            : 0.0;
    }
}
