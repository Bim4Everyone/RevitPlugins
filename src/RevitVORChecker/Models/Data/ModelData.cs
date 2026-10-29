using System.IO;

using pyRevitLabs.Json;

namespace RevitVORChecker.Models.Data;

/// <summary>
/// Данные одной модели КР, выгруженные для проверки (файл model_&lt;имя модели&gt;.json рабочей папки).
/// </summary>
internal class ModelData {
    [JsonProperty("file")]
    public ModelFileData File { get; set; } = new();

    /// <summary>
    /// Заполненные параметры «Сведений о проекте»: имя параметра → значение.
    /// </summary>
    [JsonProperty("projectInfo")]
    public Dictionary<string, string> ProjectInfo { get; set; } = new();

    /// <summary>
    /// Единицы проекта: вид величины (length, area, volume, angle) → «unit:...|acc=точность».
    /// </summary>
    [JsonProperty("units")]
    public Dictionary<string, string> Units { get; set; } = new();

    [JsonProperty("worksets")]
    public List<string> Worksets { get; set; } = new();

    [JsonProperty("levels")]
    public List<LevelData> Levels { get; set; } = new();

    [JsonProperty("links")]
    public List<LinkData> Links { get; set; } = new();

    [JsonProperty("imports")]
    public List<ImportData> Imports { get; set; } = new();

    [JsonProperty("views3d")]
    public List<View3DData> Views3D { get; set; } = new();

    [JsonProperty("materials")]
    public List<MaterialData> Materials { get; set; } = new();

    [JsonProperty("directShapes")]
    public int DirectShapeCount { get; set; }

    [JsonProperty("elements")]
    public List<ElementData> Elements { get; set; } = new();

    [JsonProperty("warnings")]
    public List<WarningData> Warnings { get; set; } = new();

    [JsonProperty("schedules")]
    public List<ScheduleData> Schedules { get; set; } = new();
}

/// <summary>
/// Сведения о файле модели.
/// </summary>
internal class ModelFileData {
    [JsonProperty("title")]
    public string Title { get; set; }

    [JsonProperty("path")]
    public string Path { get; set; }

    [JsonProperty("central")]
    public string CentralPath { get; set; }

    /// <summary>
    /// Размер файла в байтах; -1 — размер неизвестен.
    /// </summary>
    [JsonProperty("sizeBytes")]
    public long? SizeBytes { get; set; }

    [JsonProperty("workshared")]
    public bool IsWorkshared { get; set; }

    [JsonProperty("revit")]
    public string RevitVersion { get; set; }

    [JsonProperty("exportedAt")]
    public string ExportedAt { get; set; }
}

internal class LevelData {
    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// Отметка уровня, мм.
    /// </summary>
    [JsonProperty("elev")]
    public double Elevation { get; set; }

    [JsonProperty("ws")]
    public string Workset { get; set; }

    /// <summary>
    /// Отметка уровня во внутренних координатах модели (как габариты элементов), мм.
    /// </summary>
    [JsonProperty("projElev", NullValueHandling = NullValueHandling.Ignore)]
    public double? ProjectElevation { get; set; }

    /// <summary>
    /// Габарит уровня в плане, мм: [minX, minY, maxX, maxY]; null — не определён (уровень не виден на 3D-виде).
    /// </summary>
    [JsonProperty("extent", NullValueHandling = NullValueHandling.Ignore)]
    public List<double> PlanExtent { get; set; }
}

/// <summary>
/// Связь RVT модели.
/// </summary>
internal class LinkData {
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// Связь загружена; null — неизвестно (связь удалена при открытии модели для проверки).
    /// </summary>
    [JsonProperty("loaded")]
    public bool? IsLoaded { get; set; }

    [JsonProperty("attachment")]
    public string Attachment { get; set; }

    [JsonProperty("instances")]
    public int InstanceCount { get; set; }

    /// <summary>
    /// Рабочие наборы экземпляров связи.
    /// </summary>
    [JsonProperty("ws")]
    public List<string> InstanceWorksets { get; set; } = new();

    [JsonProperty("typeWs")]
    public string TypeWorkset { get; set; }

    [JsonProperty("removedOnOpen")]
    public bool RemovedOnOpen { get; set; }
}

/// <summary>
/// Импортированный или связанный файл (DWG и т.п.).
/// </summary>
internal class ImportData {
    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("linked")]
    public bool IsLinked { get; set; }

    [JsonProperty("viewSpecific")]
    public bool IsViewSpecific { get; set; }

    [JsonProperty("ws")]
    public string Workset { get; set; }
}

internal class View3DData {
    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("cropBox")]
    public bool IsCropBoxActive { get; set; }

    [JsonProperty("sectionBox")]
    public bool IsSectionBoxActive { get; set; }
}

/// <summary>
/// Предупреждение Revit.
/// </summary>
internal class WarningData {
    [JsonProperty("text")]
    public string Text { get; set; }

    [JsonProperty("sev")]
    public string Severity { get; set; }

    [JsonProperty("ids")]
    public List<long> ElementIds { get; set; } = new();
}

/// <summary>
/// Спецификация модели: фильтры и текст ячеек тела таблицы.
/// </summary>
internal class ScheduleData {
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("includeLinks")]
    public bool IncludeLinks { get; set; }

    /// <summary>
    /// Фильтры в виде «Поле ТипФильтра Значение».
    /// </summary>
    [JsonProperty("filters")]
    public List<string> Filters { get; set; } = new();

    [JsonProperty("rows")]
    public List<List<string>> Rows { get; set; } = new();
}
