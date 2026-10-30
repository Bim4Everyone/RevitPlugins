using pyRevitLabs.Json;

namespace RevitVORChecker.Models.Data;

/// <summary>
/// Элемент модели КР с параметрами, материалами и габаритом.
/// </summary>
internal class ElementData {
    /// <summary>
    /// Параметры экземпляра или типа, которые выгружаются по имени.
    /// </summary>
    /// <remarks>
    /// Параметры СМР берутся из платформы; остальных параметров ФОП (ФОП_ТИП_*, обр_ФОП_*, ФОП_Размер,
    /// ФОП_Наименование) в платформе нет (SharedParamsConfig, проверено 30.10.2026) — имена строками.
    /// Системные параметры («Толщина», «Уровень»…) выгружаются по отображаемому имени, как в спецификациях ВОР.
    /// </remarks>
    public static readonly string[] ExportedParameters = {
        PlatformParamNames.Block, PlatformParamNames.Section, PlatformParamNames.Floor, PlatformParamNames.Zone,
        PlatformParamNames.FixBuildingWorks,
        "ФОП_ТИП_Местоположение", "ФОП_ТИП_Назначение", "ВОР_Характеристики", "ФОП_ТИП_Доп. сортировка",
        "ФОП_Размер", "ФОП_ТИП_Тип материала", "ФОП_ТИП_Армирование",
        "обр_ФОП_Марка ведомости расхода", "обр_ФОП_Раздел проекта", "обр_ФОП_Марка бетона B",
        "обр_ФОП_Фильтрация 1", "обр_ФОП_Фильтрация 2", "ФОП_Наименование",
        "Толщина", "Площадка_Толщина", "Уровень", "Базовый уровень", "Зависимость сверху", "Опорный уровень"
    };

    [JsonProperty("id")]
    public long Id { get; set; }

    /// <summary>
    /// Имя категории.
    /// </summary>
    [JsonProperty("cat")]
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Встроенная категория (имя значения BuiltInCategory).
    /// </summary>
    [JsonProperty("bic")]
    public string BuiltInCategory { get; set; } = string.Empty;

    [JsonProperty("family")]
    public string? FamilyName { get; set; }

    [JsonProperty("type")]
    public string TypeName { get; set; } = string.Empty;

    [JsonProperty("ws")]
    public string? Workset { get; set; }

    [JsonProperty("level")]
    public string? LevelName { get; set; }

    [JsonProperty("inGroup")]
    public bool IsInGroup { get; set; }

    [JsonProperty("inPlace")]
    public bool IsInPlace { get; set; }

    /// <summary>
    /// Id родительского семейства для вложенных экземпляров.
    /// </summary>
    [JsonProperty("super")]
    public long? SuperComponentId { get; set; }

    [JsonProperty("phase")]
    public string? PhaseCreated { get; set; }

    [JsonProperty("demolished")]
    public string? PhaseDemolished { get; set; }

    /// <summary>
    /// Объём элемента, м³.
    /// </summary>
    [JsonProperty("vol")]
    public double? Volume { get; set; }

    /// <summary>
    /// Габарит, мм: [minX, minY, minZ, maxX, maxY, maxZ].
    /// </summary>
    [JsonProperty("bb")]
    public List<double?>? BoundingBox { get; set; }

    /// <summary>
    /// Параметры: имя → [значение, «i» — экземпляра / «t» — типа].
    /// </summary>
    [JsonProperty("p")]
    public Dictionary<string, List<string?>> Parameters { get; set; } = new();

    /// <summary>
    /// Материалы: [id материала, объём м³, площадь м²].
    /// </summary>
    [JsonProperty("mats")]
    public List<List<double?>> Materials { get; set; } = new();

    /// <summary>
    /// Материалы, нанесённые инструментом «Краска».
    /// </summary>
    [JsonProperty("paint")]
    public List<long> PaintMaterialIds { get; set; } = new();

    /// <summary>
    /// Значение параметра или null, если параметра нет.
    /// </summary>
    public string? GetParameter(string name) {
        return Parameters.TryGetValue(name, out List<string?>? value) && value.Count > 0 ? value[0] : null;
    }

    /// <summary>
    /// Значение параметра или пустая строка.
    /// </summary>
    public string GetParameterOrEmpty(string name) {
        return GetParameter(name) ?? "";
    }

    public IEnumerable<long> GetMaterialIds() {
        return Materials
            .Where(material => material.Count > 0 && material[0].HasValue)
            .Select(material => (long) material[0].GetValueOrDefault());
    }
}
