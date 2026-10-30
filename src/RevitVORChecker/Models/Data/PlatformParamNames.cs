using dosymep.Bim4Everyone.SharedParams;

namespace RevitVORChecker.Models.Data;

/// <summary>
/// Имена общих параметров, которые есть в платформе (<see cref="SharedParamsConfig"/>).
/// </summary>
/// <remarks>
/// В Revit настройки параметров загружает платформа (<see cref="SharedParamsConfig.Instance"/>).
/// Вне Revit (сверка правил на сохранённых данных) экземпляра нет — берутся настройки по умолчанию.
/// Имена читаются один раз: правила обращаются к ним для каждого элемента.
/// Остальные параметры ФОП, которых в платформе нет, записаны строками там, где используются.
/// </remarks>
internal static class PlatformParamNames {
    private static readonly SharedParamsConfig _config = SharedParamsConfig.Instance ?? SharedParamsConfig.GetDefaultConfig();

    /// <summary>
    /// ФОП_Блок СМР.
    /// </summary>
    public static readonly string Block = _config.BuildingWorksBlock.Name;

    /// <summary>
    /// ФОП_Секция СМР.
    /// </summary>
    public static readonly string Section = _config.BuildingWorksSection.Name;

    /// <summary>
    /// ФОП_Этаж СМР.
    /// </summary>
    public static readonly string Floor = _config.BuildingWorksLevel.Name;

    /// <summary>
    /// ФОП_Зона СМР.
    /// </summary>
    public static readonly string Zone = _config.BuildingWorksZone.Name;

    /// <summary>
    /// ФОП_Фиксация координаты СМР.
    /// </summary>
    public static readonly string FixBuildingWorks = _config.FixBuildingWorks.Name;

    /// <summary>
    /// ФОП_МТР_Наименование главы.
    /// </summary>
    public static readonly string MtrChapterTitle = _config.MtrChapterTitle.Name;

    /// <summary>
    /// ФОП_МТР_Наименование работы.
    /// </summary>
    public static readonly string MtrWorkTitle = _config.MtrWorkTitle.Name;

    /// <summary>
    /// ФОП_МТР_Единица измерения.
    /// </summary>
    public static readonly string MtrMeasurementUnit = _config.MtrMeasurementUnit.Name;

    /// <summary>
    /// ФОП_МТР_Тип подсчета.
    /// </summary>
    public static readonly string MtrCountingType = _config.MtrCountingType.Name;
}
