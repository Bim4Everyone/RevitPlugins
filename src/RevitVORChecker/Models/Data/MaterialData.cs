using pyRevitLabs.Json;

namespace RevitVORChecker.Models.Data;

/// <summary>
/// Материал модели с параметрами, которые нужны для проверки ВОР.
/// </summary>
internal class MaterialData {
    public static string ChapterNameParameter => PlatformParamNames.MtrChapterTitle;
    public static string WorkNameParameter => PlatformParamNames.MtrWorkTitle;
    public static string UnitParameter => PlatformParamNames.MtrMeasurementUnit;
    public static string CountTypeParameter => PlatformParamNames.MtrCountingType;

    /// <summary>
    /// Параметры материала, которые выгружаются по имени.
    /// </summary>
    public static readonly string[] ExportedParameters = {
        ChapterNameParameter,
        WorkNameParameter,
        UnitParameter,
        CountTypeParameter,

        // этих параметров в платформе нет (SharedParamsConfig, проверено 30.10.2026) — имена строками
        "ФОП_МТР_Наименование",
        "ФОП_МТР_Индекс материала"
    };

    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("class")]
    public string? MaterialClass { get; set; }

    [JsonProperty("keynote")]
    public string? Keynote { get; set; }

    [JsonProperty("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Прочие параметры материала (имена из <see cref="ExportedParameters"/>).
    /// </summary>
    [JsonExtensionData]
    public IDictionary<string, pyRevitLabs.Json.Linq.JToken> ExtraParameters { get; set; } =
        new Dictionary<string, pyRevitLabs.Json.Linq.JToken>();

    public string? GetParameter(string name) {
        return ExtraParameters.TryGetValue(name, out var token) && token.Type != pyRevitLabs.Json.Linq.JTokenType.Null
            ? token.ToString()
            : null;
    }

    public void SetParameter(string name, string? value) {
        ExtraParameters[name] = value == null
            ? pyRevitLabs.Json.Linq.JValue.CreateNull()
            : new pyRevitLabs.Json.Linq.JValue(value);
    }
}
