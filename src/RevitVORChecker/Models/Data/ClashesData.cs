using pyRevitLabs.Json;

namespace RevitVORChecker.Models.Data;

/// <summary>
/// Пересечения и дубли элементов одной модели (файл clashes_&lt;имя модели&gt;.json рабочей папки).
/// </summary>
internal class ClashesData {
    [JsonProperty("file")]
    public string FileTitle { get; set; } = string.Empty;

    /// <summary>
    /// Допуск глубины пересечения, мм.
    /// </summary>
    [JsonProperty("tolerance_mm")]
    public double ToleranceMm { get; set; } = 50;

    [JsonProperty("elements")]
    public int ElementCount { get; set; }

    [JsonProperty("pairsChecked")]
    public int CheckedPairCount { get; set; }

    /// <summary>
    /// Число пар, которые не удалось проанализировать (ошибки геометрии Revit).
    /// </summary>
    [JsonProperty("geomErrors")]
    public int GeometryErrorCount { get; set; }

    [JsonProperty("seconds")]
    public int Seconds { get; set; }

    [JsonProperty("duplicates")]
    public List<DuplicateData> Duplicates { get; set; } = new();

    [JsonProperty("clashes")]
    public List<ClashData> Clashes { get; set; } = new();
}

/// <summary>
/// Два одинаковых элемента в одном месте.
/// </summary>
internal class DuplicateData {
    [JsonProperty("a")]
    public long FirstId { get; set; }

    [JsonProperty("b")]
    public long SecondId { get; set; }

    [JsonProperty("cat")]
    public string Category { get; set; } = string.Empty;

    [JsonProperty("type")]
    public string TypeName { get; set; } = string.Empty;
}

/// <summary>
/// Пересечение тел двух элементов.
/// </summary>
internal class ClashData {
    [JsonProperty("a")]
    public long FirstId { get; set; }

    [JsonProperty("b")]
    public long SecondId { get; set; }

    /// <summary>
    /// Группа первого элемента: «ЖБ» или «Прочее».
    /// </summary>
    [JsonProperty("ga")]
    public string FirstGroup { get; set; } = string.Empty;

    [JsonProperty("gb")]
    public string SecondGroup { get; set; } = string.Empty;

    /// <summary>
    /// «Категория: Тип» первого элемента.
    /// </summary>
    [JsonProperty("ta")]
    public string FirstTitle { get; set; } = string.Empty;

    [JsonProperty("tb")]
    public string SecondTitle { get; set; } = string.Empty;

    [JsonProperty("vol_m3")]
    public double VolumeM3 { get; set; }

    [JsonProperty("depth_mm")]
    public double DepthMm { get; set; }

    [JsonProperty("joined")]
    public bool IsJoined { get; set; }
}
