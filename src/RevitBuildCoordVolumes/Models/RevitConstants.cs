using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone.SharedParams;

using RevitBuildCoordVolumes.Models.Enums;

namespace RevitBuildCoordVolumes.Models;
internal static class RevitConstants {

    // Категории, обрабатываемые плагином в качестве основы для построения
    public static readonly ICollection<BuiltInCategory> SlabCategories = [
        BuiltInCategory.OST_StructuralFoundation,
        BuiltInCategory.OST_Floors];

    // Категория объемных элементов
    public static BuiltInCategory SourceVolumeCategory = BuiltInCategory.OST_GenericModel;

    // Самое распространенное значения для записи координат
    public const string TypeZone = "Координаты СМР";

    // Наиболее оптимальная сторона фигуры для поиска плиты в миллиметрах
    public static double SearchSide = 50;

    // Параметр, значения которого используется дли идентификации зон
    //public static RevitParam ZoneParama = SharedParamsConfig.Instance.Description;


    // Утвержденные карты параметров, которые используются в проектах разных дисциплин
    private static readonly List<ParamMap> _allParamMaps = [
        new ParamMap {
            Type = ParamType.DescriptionParam,
            SourceParam = SharedParamsConfig.Instance.Description,
            TargetParam = SharedParamsConfig.Instance.Description
        },
        new ParamMap {
            Type = ParamType.BlockParam,
            SourceParam = SharedParamsConfig.Instance.BuildingWorksBlock,
            TargetParam = SharedParamsConfig.Instance.BuildingWorksBlock
        },
        new ParamMap {
            Type = ParamType.SectionParam,
            SourceParam = SharedParamsConfig.Instance.BuildingWorksSection,
            TargetParam = SharedParamsConfig.Instance.BuildingWorksSection
        },
        new ParamMap {
            Type = ParamType.FloorParam,
            SourceParam = SharedParamsConfig.Instance.BuildingWorksLevel,
            TargetParam = SharedParamsConfig.Instance.BuildingWorksLevel
        },
        new ParamMap {
            Type = ParamType.FloorDEParam,
            SourceParam = SharedParamsConfig.Instance.BuildingWorksLevelCurrency,
            TargetParam = SharedParamsConfig.Instance.BuildingWorksLevelCurrency
        },
    ];

    // Части имен типоразмеров плит перекрытий КР
    private static readonly List<string> _slabTypeNames = [
        "(КР)",
        "КЖ",
        "кж",
        "ЖБ",
        "жб",
        "Железобетон",
        "железобетон",
        "Монолит",
        "монолит"];

    /// <summary>
    /// Метод получения карты параметров
    /// </summary>
    public static List<ParamMap> GetDefaultParamMaps() {
        return _allParamMaps;
    }
    /// <summary>
    /// Метод получения частей имен типоразмеров перекрытий
    /// </summary>
    public static List<string> GetDefaultSlabTypeNames() {
        return _slabTypeNames;
    }
}
