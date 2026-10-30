using dosymep.WPF.ViewModels;

using RevitVORChecker.Models.Project;

namespace RevitVORChecker.ViewModels;

/// <summary>
/// Строка табл. 2.6.4 в окне «Данные проекта».
/// </summary>
/// <remarks>
/// Значения редактируются текстом: целое число кг/м³ или прочерк (нормы нет).
/// У паркинга в ЗнП один расход на оба столбца и допустимое значение в особом случае.
/// </remarks>
internal class ReinforcementRowViewModel : BaseViewModel {
    private readonly ReinforcementTableRow _row;
    private string _thickness;
    private string _firstValue;
    private string _secondValue;
    private string _alternativeValue;
    private bool _isFirstValueUsed;
    private bool _isSecondValueUsed;

    public ReinforcementRowViewModel(ReinforcementTableRow row, string groupTitle) {
        _row = row;
        GroupTitle = groupTitle;
        _thickness = row.Thickness;
        _firstValue = ToText(row.FirstValue);
        _secondValue = row.IsParking ? _firstValue : ToText(row.SecondValue);
        _alternativeValue = row.IsParking && row.AlternativeCondition != null ? ToText(row.SecondValue) : "";
    }

    public string Key => _row.Key;

    /// <summary>
    /// Группа строк: «Жилые корпуса» или «Паркинг».
    /// </summary>
    public string GroupTitle { get; }

    public string Number => _row.Number;

    public string ConstructionType => _row.ConstructionType;

    public bool IsParking => _row.IsParking;

    public string AlternativeCondition => _row.AlternativeCondition ?? "";

    /// <summary>
    /// Столбец 10–17 редактируется только у жилых корпусов.
    /// </summary>
    public bool IsSecondValueReadOnly => IsParking;

    public bool IsAlternativeValueReadOnly => !IsParking || _row.AlternativeCondition == null;

    public string Thickness {
        get => _thickness;
        set => RaiseAndSetIfChanged(ref _thickness, value);
    }

    public string FirstValue {
        get => _firstValue;
        set {
            RaiseAndSetIfChanged(ref _firstValue, value);
            if(IsParking) {
                SecondValue = value;
            }
        }
    }

    public string SecondValue {
        get => _secondValue;
        set => RaiseAndSetIfChanged(ref _secondValue, value);
    }

    public string AlternativeValue {
        get => _alternativeValue;
        set => RaiseAndSetIfChanged(ref _alternativeValue, value);
    }

    /// <summary>
    /// По значению столбца 7–9 будет проверка (подсветка).
    /// </summary>
    public bool IsFirstValueUsed {
        get => _isFirstValueUsed;
        set => RaiseAndSetIfChanged(ref _isFirstValueUsed, value);
    }

    /// <summary>
    /// По значению столбца 10–17 будет проверка (подсветка).
    /// </summary>
    public bool IsSecondValueUsed {
        get => _isSecondValueUsed;
        set => RaiseAndSetIfChanged(ref _isSecondValueUsed, value);
    }

    /// <summary>
    /// Разбирает значение ячейки: целое больше нуля или прочерк / пусто (null).
    /// </summary>
    public static bool TryParse(string? text, out int? value) {
        string trimmed = (text ?? "").Trim();
        value = null;
        if(trimmed.Length == 0 || trimmed == "-" || trimmed == "—" || trimmed == "–") {
            return true;
        }

        if(int.TryParse(trimmed, out int number) && number > 0) {
            value = number;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Строка таблицы с введёнными значениями или null, если значение не число.
    /// </summary>
    public ReinforcementTableRow? ToTableRow() {
        if(!TryParse(FirstValue, out int? firstValue)
           || (!IsParking && !TryParse(SecondValue, out _))
           || !TryParse(AlternativeValue, out _)) {
            return null;
        }

        TryParse(IsParking ? AlternativeValue : SecondValue, out int? secondValue);
        return new ReinforcementTableRow(
            _row.Key,
            _row.Number,
            _row.ConstructionType,
            (Thickness ?? "").Trim(),
            firstValue,
            secondValue,
            _row.AlternativeCondition);
    }

    private static string ToText(int? value) {
        return value.HasValue ? value.Value.ToString() : "-";
    }
}
