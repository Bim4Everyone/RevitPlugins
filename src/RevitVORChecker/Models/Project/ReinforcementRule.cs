using pyRevitLabs.Json;

namespace RevitVORChecker.Models.Project;

/// <summary>
/// Правило проверки ФОП_ТИП_Армирование: условия отбора элементов и норматив расхода, кг/м³.
/// </summary>
/// <remarks>
/// Пустое условие (null) не ограничивает отбор.
/// </remarks>
internal class ReinforcementRule {
    /// <summary>
    /// Начала значений ФОП_Блок СМР.
    /// </summary>
    [JsonProperty("block", NullValueHandling = NullValueHandling.Ignore)]
    public List<string>? Blocks { get; set; }

    /// <summary>
    /// Циклы возведения — начало имени типа до «_» (Ф, НН, ВН).
    /// </summary>
    [JsonProperty("cycle", NullValueHandling = NullValueHandling.Ignore)]
    public List<string>? Cycles { get; set; }

    /// <summary>
    /// Фрагменты имени типа.
    /// </summary>
    [JsonProperty("keys", NullValueHandling = NullValueHandling.Ignore)]
    public List<string>? TypeKeys { get; set; }

    /// <summary>
    /// Значения ФОП_ТИП_Назначение.
    /// </summary>
    [JsonProperty("nazn", NullValueHandling = NullValueHandling.Ignore)]
    public List<string>? Purposes { get; set; }

    /// <summary>
    /// Значения ФОП_Этаж СМР.
    /// </summary>
    [JsonProperty("floors", NullValueHandling = NullValueHandling.Ignore)]
    public List<string>? Floors { get; set; }

    [JsonProperty("value")]
    public double Value { get; set; }

    /// <summary>
    /// Значения, которые допускаются в особом случае.
    /// </summary>
    [JsonProperty("alt", NullValueHandling = NullValueHandling.Ignore)]
    public List<double>? AlternativeValues { get; set; }

    [JsonProperty("note")]
    public string Note { get; set; } = string.Empty;
}
