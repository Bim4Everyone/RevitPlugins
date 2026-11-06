using System;
using System.Globalization;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

namespace RevitValueModifier.Models;
/// <summary>
/// Преобразует строку, полученную по маске, в значение для записи в параметр нужного типа
/// и форматирует подготовленное значение для отображения пользователю
/// </summary>
internal class ParamValueParser {
    private const double _integerTolerance = 1e-9;
    private static readonly string[] _trueValues = ["да", "yes", "true", "1", "истина"];
    private static readonly string[] _falseValues = ["нет", "no", "false", "0", "ложь"];

    private readonly Units _units;
    private readonly string _trueText;
    private readonly string _falseText;

    public ParamValueParser(Document document, ILocalizationService localizationService) {
        _units = document.GetUnits();
        _trueText = localizationService.GetLocalizedString("MainWindow.True");
        _falseText = localizationService.GetLocalizedString("MainWindow.False");
    }

    /// <summary>
    /// Преобразует строку в значение для записи в параметр
    /// </summary>
    /// <param name="text">Строка, полученная по маске</param>
    /// <param name="targetParameter">Параметр, в который будет записано значение</param>
    /// <param name="value">string, int или double (во внутренних единицах Revit)</param>
    /// <returns>True, если строку удалось преобразовать</returns>
    public bool TryParse(string text, RevitParameter targetParameter, out object value) {
        value = null;
        text ??= string.Empty;
        switch(targetParameter.ValueKind) {
            case ParamValueKind.Text:
                value = text;
                return true;
            case ParamValueKind.Integer when TryParseInteger(text, out int integer):
                value = integer;
                return true;
            case ParamValueKind.YesNo when TryParseYesNo(text, out int yesNo):
                value = yesNo;
                return true;
            case ParamValueKind.Double when TryParseDouble(text, targetParameter.DataType, out double number):
                value = number;
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// Берет значение исходного параметра без преобразования в строку, если типы данных совпадают.
    /// Так не теряется точность из-за округления в формате единиц проекта
    /// </summary>
    /// <param name="sourceParameter">Параметр элемента, указанный в маске</param>
    /// <param name="targetParameter">Параметр, в который будет записано значение</param>
    /// <param name="value">int или double (во внутренних единицах Revit)</param>
    /// <returns>True, если значение можно скопировать без преобразования</returns>
    public bool TryGetExactValue(Parameter sourceParameter, RevitParameter targetParameter, out object value) {
        value = null;
        if(!sourceParameter.HasValue
            || sourceParameter.StorageType != targetParameter.ParamStorageType
            || sourceParameter.Definition.GetDataType() != targetParameter.DataType) {
            return false;
        }
        switch(targetParameter.ValueKind) {
            case ParamValueKind.Integer:
            case ParamValueKind.YesNo:
                value = sourceParameter.AsInteger();
                return true;
            case ParamValueKind.Double:
                value = sourceParameter.AsDouble();
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// Форматирует подготовленное значение так, как его покажет Revit после записи
    /// </summary>
    public string Format(object value, RevitParameter targetParameter) {
        return targetParameter.ValueKind switch {
            ParamValueKind.Double => UnitFormatUtils.Format(
                _units,
                targetParameter.DataType,
                (double) value,
                false,
                new FormatValueOptions() { AppendUnitSymbol = true }),
            ParamValueKind.YesNo => (int) value == 1 ? _trueText : _falseText,
            ParamValueKind.Integer => ((int) value).ToString(CultureInfo.InvariantCulture),
            _ => value?.ToString() ?? string.Empty
        };
    }

    private bool TryParseDouble(string text, ForgeTypeId dataType, out double value) {
        // Разбор по настройкам единиц проекта: учитывает десятичный разделитель,
        // разделитель разрядов и обозначения единиц ("1500 мм", "1,5 м")
        // и возвращает значение во внутренних единицах Revit
        if(UnitFormatUtils.TryParse(_units, dataType, text.Trim(), out value)) {
            return true;
        }
        // Запасной вариант: число без обозначения единиц с любым десятичным разделителем.
        // Считается, что оно указано в единицах, заданных в проекте для этого типа данных
        if(TryParseNumber(text, out double displayValue)) {
            ForgeTypeId unitTypeId = _units.GetFormatOptions(dataType).GetUnitTypeId();
            value = UnitUtils.ConvertToInternalUnits(displayValue, unitTypeId);
            return true;
        }
        return false;
    }

    private static bool TryParseInteger(string text, out int value) {
        string normalized = RemoveWhitespaces(text);
        if(int.TryParse(normalized, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out value)) {
            return true;
        }
        // Значения вида "12,00" из числовых параметров принимаются, если дробная часть нулевая
        if(TryParseNumber(normalized, out double number)
            && Math.Abs(number - Math.Round(number)) < _integerTolerance
            && number >= int.MinValue
            && number <= int.MaxValue) {
            value = (int) Math.Round(number);
            return true;
        }
        value = 0;
        return false;
    }

    private bool TryParseYesNo(string text, out int value) {
        string normalized = text.Trim();
        if(IsOneOf(normalized, _trueText) || _trueValues.Any(v => IsOneOf(normalized, v))) {
            value = 1;
            return true;
        }
        if(IsOneOf(normalized, _falseText) || _falseValues.Any(v => IsOneOf(normalized, v))) {
            value = 0;
            return true;
        }
        value = 0;
        return false;
    }

    private static bool TryParseNumber(string text, out double value) {
        string normalized = RemoveWhitespaces(text).Replace(',', '.');
        return double.TryParse(
            normalized,
            NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture,
            out value);
    }

    private static bool IsOneOf(string text, string expected) {
        return !string.IsNullOrEmpty(expected)
            && string.Equals(text, expected, StringComparison.OrdinalIgnoreCase);
    }

    private static string RemoveWhitespaces(string text) {
        // char.IsWhiteSpace учитывает и неразрывные пробелы, которые Revit использует как разделитель разрядов
        return new string(text.Where(c => !char.IsWhiteSpace(c)).ToArray());
    }
}
