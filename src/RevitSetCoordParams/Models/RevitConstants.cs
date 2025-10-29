using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;

using RevitSetCoordParams.Models.Enums;

namespace RevitSetCoordParams.Models;
internal static class RevitConstants {
    // Начальный диаметр сферы в миллиметрах
    public static double StartDiameterSearchSphereMm = 30;
    // Средне-статистическое максимальное расстояние от удаленных элементов в моделях
    public const double MaxDiameterSearchSphereMm = 2000;
    // Наиболее оптимальный шаг увеличения поисковой сферы
    public const double StepDiameterSearchSphereMm = 200;
    // Использование поиска по умолчанию включено
    public const bool Search = true;
    // Самое распространенное значения для записи координат
    public const string TypeModel = "Координаты СМР";
    // Ключевое слово, для фильтрации объемных элементов
    public const string TypeModelPartName = "СМР";
    // Имя рабочего набора, элементы которого не будут обработаны
    public const string WorksetExcludeName = "99_Немоделируемые элементы";
    // Ключевая строка, по которому ищется координационный файл
    public const string CoordFilePartName = "KOORD";
    // Параметр, значения которого используется дли идентификации объемных элементов
    public static RevitParam SourceVolumeParam = SharedParamsConfig.Instance.Description;
    // Категория объемных элементов
    public static BuiltInCategory SourceVolumeCategory = BuiltInCategory.OST_GenericModel;


    // Утвержденные категории, которые используются в проектах разных дисциплин
    private static readonly List<BuiltInCategory> _allCategories = [
        BuiltInCategory.OST_CableTray,
        BuiltInCategory.OST_Casework,
        BuiltInCategory.OST_Ceilings,
        BuiltInCategory.OST_Curtain_Systems,
        BuiltInCategory.OST_CurtainWallMullions,
        BuiltInCategory.OST_CurtainWallPanels,
        BuiltInCategory.OST_CommunicationDevices,
        BuiltInCategory.OST_Conduit,
        BuiltInCategory.OST_CurtainWallPanels,
        BuiltInCategory.OST_Doors,
        BuiltInCategory.OST_DuctCurves,
        BuiltInCategory.OST_DuctFitting,
        BuiltInCategory.OST_DuctAccessory,
        BuiltInCategory.OST_DuctInsulations,
        BuiltInCategory.OST_DuctTerminal,
        BuiltInCategory.OST_ElectricalEquipment,
        BuiltInCategory.OST_ElectricalFixtures,
        BuiltInCategory.OST_Floors,
        BuiltInCategory.OST_Furniture,
        BuiltInCategory.OST_FurnitureSystems,
        BuiltInCategory.OST_GenericModel,
        BuiltInCategory.OST_LightingDevices,
        BuiltInCategory.OST_LightingFixtures,
        BuiltInCategory.OST_Mass,
        BuiltInCategory.OST_MechanicalEquipment,
        BuiltInCategory.OST_PlaceHolderPipes,
        BuiltInCategory.OST_PipeCurves,
        BuiltInCategory.OST_PipeFitting,
        BuiltInCategory.OST_PipeAccessory,
        BuiltInCategory.OST_PipeInsulations,
        BuiltInCategory.OST_Parking,
        BuiltInCategory.OST_Planting,
        BuiltInCategory.OST_PlumbingFixtures,
        BuiltInCategory.OST_RailingSystemRail,
        BuiltInCategory.OST_Railings,
        BuiltInCategory.OST_Ramps,
        BuiltInCategory.OST_Roads,
        BuiltInCategory.OST_Roofs,
        BuiltInCategory.OST_SecurityDevices,
        BuiltInCategory.OST_Sprinklers,
        BuiltInCategory.OST_Stairs,
        BuiltInCategory.OST_StairsRailing,
        BuiltInCategory.OST_StructuralColumns,
        BuiltInCategory.OST_StructuralFoundation,
        BuiltInCategory.OST_StructuralFraming,
        BuiltInCategory.OST_Topography,
        BuiltInCategory.OST_Walls,
        BuiltInCategory.OST_Windows];

    // Утвержденные карты параметров, которые используются в проектах разных дисциплин
    private static readonly List<ParamMap> _allParamMaps = [
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
        new ParamMap {
            Type = ParamType.BlockingParam,
            SourceParam = null,
            TargetParam = SharedParamsConfig.Instance.FixBuildingWorks
        },
    ];

    // Метод получения карты параметров
    public static List<ParamMap> GetDefaultParamMaps() {
        return _allParamMaps;
    }

    // Метод получения категорий
    public static List<BuiltInCategory> GetDefaultBuiltInCategories() {
        return _allCategories;
    }
}
