using pyRevitLabs.Json;

namespace RevitVORChecker.Models.Data;

/// <summary>
/// Спецификации сводной модели со связями (файл schedules.json рабочей папки) — для сверки с выгрузками ВОР в Excel.
/// </summary>
internal class ScheduleTablesData {
    [JsonProperty("file")]
    public string FileTitle { get; set; }

    [JsonProperty("schedules")]
    public List<ScheduleTableData> Schedules { get; set; } = new();

    [JsonProperty("log")]
    public List<string> Log { get; set; } = new();
}

/// <summary>
/// Текст ячеек шапки и тела спецификации.
/// </summary>
internal class ScheduleTableData {
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// Спецификации нет в модели.
    /// </summary>
    [JsonProperty("missing")]
    public bool IsMissing { get; set; }

    [JsonProperty("includeLinksOriginal")]
    public bool IncludedLinksOriginally { get; set; }

    [JsonProperty("header")]
    public List<List<string>> HeaderRows { get; set; } = new();

    [JsonProperty("body")]
    public List<List<string>> BodyRows { get; set; } = new();
}
