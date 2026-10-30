namespace RevitVORChecker.Models.Project;

/// <summary>
/// Строка табл. 2.6.4 ЗнП «Расход арматуры на 1 куб. м бетона, не более».
/// </summary>
/// <remarks>
/// Для жилых корпусов: <see cref="FirstValue"/> — расход для 7–9-этажных домов, <see cref="SecondValue"/> — для 10–17-этажных.
/// Для паркинга: <see cref="FirstValue"/> — расход, <see cref="SecondValue"/> — допустимое значение в особом случае.
/// </remarks>
internal class ReinforcementTableRow {
    public ReinforcementTableRow(
        string key,
        string number,
        string constructionType,
        string thickness,
        int? firstValue,
        int? secondValue,
        string? alternativeCondition = null) {

        Key = key;
        Number = number;
        ConstructionType = constructionType;
        Thickness = thickness;
        FirstValue = firstValue;
        SecondValue = secondValue;
        AlternativeCondition = alternativeCondition;
    }

    /// <summary>
    /// Ключ строки: K1…K11 — жилые корпуса, P1…P6 — паркинг.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Номер строки в ЗнП.
    /// </summary>
    public string Number { get; }

    public string ConstructionType { get; }

    public string Thickness { get; set; }

    public int? FirstValue { get; set; }

    public int? SecondValue { get; set; }

    /// <summary>
    /// Когда для паркинга допускается значение <see cref="SecondValue"/>; null — особого случая нет.
    /// </summary>
    public string? AlternativeCondition { get; }

    public bool IsParking => Key.StartsWith("P");
}
