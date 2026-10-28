using pyRevitLabs.Json;

namespace RevitVORChecker.Models.Project;

/// <summary>
/// Данные проекта из задания на проектирование (ЗнП), которые нельзя взять из моделей.
/// </summary>
/// <remarks>
/// Хранятся в «&lt;папка с моделями&gt;\Локальная проверка\Данные проекта.json».
/// Формат совместим с project.json эталонной проверки.
/// </remarks>
internal class ProjectCard {
    public const string KnowledgeBaseFoundationRule = "kb";
    public const string ParkingFoundationRule = "parking_if_2plus";

    /// <summary>
    /// Наименование объекта для шапки реестра.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; } = "";

    /// <summary>
    /// Номер АТС по ЗнП.
    /// </summary>
    [JsonProperty("ats")]
    public string AtsNumber { get; set; } = "5.0";

    /// <summary>
    /// Дата ЗнП в формате ГГГГ-ММ-ДД.
    /// </summary>
    [JsonProperty("znp_date")]
    public string AssignmentDate { get; set; }

    /// <summary>
    /// Стадия проектирования: «П» или «Р».
    /// </summary>
    [JsonProperty("stage")]
    public string Stage { get; set; } = "П";

    /// <summary>
    /// Этажность жилых корпусов (текст из <see cref="StoreysOptions.Titles"/>).
    /// </summary>
    [JsonProperty("storeys")]
    public string Storeys { get; set; }

    /// <summary>
    /// К чему относится фундаментная плита корпуса при 2 и более подземных этажах.
    /// </summary>
    [JsonProperty("fp_rule")]
    public string FoundationSlabRule { get; set; } = KnowledgeBaseFoundationRule;

    /// <summary>
    /// Знаков после запятой, при округлении до которых объём пересечения считается нулевым.
    /// </summary>
    [JsonProperty("clash_volume_decimals")]
    public int ClashVolumeDecimals { get; set; } = 2;

    /// <summary>
    /// Правила проверки ФОП_ТИП_Армирование, применяются сверху вниз.
    /// </summary>
    [JsonProperty("reinforcement")]
    public List<ReinforcementRule> ReinforcementRules { get; set; } = new();

    /// <summary>
    /// Значения табл. 2.6.4 ЗнП, из которых построены правила.
    /// </summary>
    [JsonProperty("reinf_table")]
    public List<ReinforcementTableValue> ReinforcementTable { get; set; } = new();

    /// <summary>
    /// Корпуса до 9 этажей при разной этажности (значения ФОП_Блок СМР).
    /// </summary>
    [JsonProperty("low_blocks")]
    public List<string> LowRiseBlocks { get; set; } = new();

    /// <summary>
    /// Путь к Классификатору видов работ, если он задан в данных проекта.
    /// </summary>
    [JsonProperty("classifier_file", NullValueHandling = NullValueHandling.Ignore)]
    public string ClassifierFile { get; set; }

    [JsonProperty("_saved", NullValueHandling = NullValueHandling.Ignore)]
    public string SavedAt { get; set; }

    /// <summary>
    /// Краткое описание для главного окна.
    /// </summary>
    public string GetSummary() {
        return $"«{Name}», АТС {AtsNumber}, ЗнП от {AssignmentDate}, стадия {Stage}, "
               + $"правил армирования: {ReinforcementRules.Count}";
    }
}

/// <summary>
/// Значения одной строки табл. 2.6.4, сохраняемые в данных проекта.
/// </summary>
internal class ReinforcementTableValue {
    [JsonProperty("key")]
    public string Key { get; set; }

    [JsonProperty("thick")]
    public string Thickness { get; set; }

    [JsonProperty("a")]
    public int? FirstValue { get; set; }

    [JsonProperty("b")]
    public int? SecondValue { get; set; }
}
