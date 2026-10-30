using pyRevitLabs.Json;

namespace RevitVORChecker.Models.Findings;

/// <summary>
/// Замечание реестра: общий текст и записи по файлам моделей.
/// </summary>
internal class Finding {
    /// <summary>
    /// Ключ раздела реестра (<see cref="ReportSections"/>).
    /// </summary>
    [JsonProperty("section")]
    public string Section { get; set; } = string.Empty;

    [JsonProperty("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Вид замечания (<see cref="FindingKinds"/>).
    /// </summary>
    [JsonProperty("kind")]
    public string Kind { get; set; } = string.Empty;

    [JsonProperty("text")]
    public string Text { get; set; } = string.Empty;

    [JsonProperty("entries")]
    public List<FindingEntry> Entries { get; set; } = new();

    /// <summary>
    /// Замечание получено сверкой выгрузок ВОР в Excel.
    /// </summary>
    [JsonProperty("from_excel", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public bool IsFromExcel { get; set; }

    /// <summary>
    /// Таблица расхождений, которая выводится в реестр под текстом замечания.
    /// </summary>
    [JsonProperty("table", NullValueHandling = NullValueHandling.Ignore)]
    public FindingTable? Table { get; set; }
}

/// <summary>
/// Запись замечания по одному файлу модели.
/// </summary>
internal class FindingEntry {
    [JsonProperty("file")]
    public string? FileName { get; set; }

    [JsonProperty("detail")]
    public string? Detail { get; set; }

    [JsonProperty("ids")]
    public List<long> ElementIds { get; set; } = new();

    /// <summary>
    /// Путь к скриншоту элементов.
    /// </summary>
    [JsonProperty("image")]
    public string? ImagePath { get; set; }
}

/// <summary>
/// Таблица расхождений в замечании.
/// </summary>
internal class FindingTable {
    [JsonProperty("header")]
    public List<string> Header { get; set; } = new();

    [JsonProperty("rows")]
    public List<List<string>> Rows { get; set; } = new();
}

/// <summary>
/// Виды замечаний.
/// </summary>
internal static class FindingKinds {
    public const string Remark = "замечание";
    public const string Clarify = "уточнить";
    public const string Info = "инфо";
}
