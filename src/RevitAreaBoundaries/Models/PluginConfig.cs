using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Serializers;

using pyRevitLabs.Json;

using RevitAreaBoundaries.Models.Enums;
using RevitAreaBoundaries.Settings;

namespace RevitAreaBoundaries.Models;

/// <summary>
/// Класс конфигурации плагина.
/// (Если не используется удалить)
/// </summary>
internal class PluginConfig : ProjectConfig<RevitSettings> {
    /// <summary>
    /// Системное свойство конфигурации. (Не трогать)
    /// </summary>
    [JsonIgnore]
    public override string ProjectConfigPath { get; set; }

    /// <summary>
    /// Системное свойство конфигурации. (Не трогать)
    /// </summary>
    [JsonIgnore]
    public override IConfigSerializer Serializer { get; set; }

    /// <summary>
    /// Метод создания конфигурации плагина.
    /// </summary>
    /// <returns>
	/// <param name="configSerializer">Сериализатор конфигурации.</param>
    /// Возвращает прочитанную конфигурацию плагина, либо созданный конфиг по умолчанию.
    /// </returns>
    public static PluginConfig GetPluginConfig(IConfigSerializer configSerializer) {
        return new ProjectConfigBuilder()
            .SetSerializer(configSerializer)
            .SetPluginName(nameof(RevitAreaBoundaries))
            .SetRevitVersion(ModuleEnvironment.RevitVersion)
            .SetProjectConfigName(nameof(PluginConfig) + ".json")
            .Build<PluginConfig>();
    }
}

/// <summary>
/// Настройки проекта.
/// В настройках проекта обычно хранится выбор пользователя в основном окне плагина.
/// </summary>
/// <remarks>
/// Проектом по умолчанию является текст до первого нижнего подчеркивания.
/// <see cref="ProjectConfig" />
/// https://github.com/dosymep/dosymep.Revit/blob/master/src/dosymep.Bim4Everyone/ProjectConfigs/ProjectConfig.cs#L102
/// Если плагин работает без открытых проектов,
/// то требуется данный класс удалять из проекта,
/// как сделано в плагине RevitServerFolders
/// https://github.com/Bim4Everyone/RevitPlugins/blob/master/src/RevitServerFolders/Models/PluginConfig.cs#L8
/// </remarks>
internal class RevitSettings : ProjectSettings {
    /// <summary>
    /// Наименование проекта. Системное свойство. (Не трогать)
    /// </summary>
    public override string ProjectName { get; set; }
    
    /// <summary>
    /// Сохраняемое свойство для примера, нужно его заменить своими настройками.
    /// </summary>
    public ConfigSettings ConfigSettings { get; set; }
}

public class SystemPluginConfig {
    // Алгоритм по умолчанию
    public readonly AlgorithmType DefaultAlgorithmType = AlgorithmType.OuterBoundary;
    // Параметр сортировки видов по умолчанию
    public readonly string DefaultGroupParamName = "_Группа Видов";
    // Высота сечения от уровня по умолчанию
    public readonly double DefaultSectionHeightMm = 1200;
    // Высота отступа от сечения по умолчанию
    public readonly double DefaultSectionHeightOffsetMm = 100;
    // Коллекция видов по умолчанию
    public readonly List<ElementId> DefaultListViewPlans = [];
    // Коллекция типов по умолчанию
    public readonly List<ElementId> DefaultListTypes = [];
    // Значения для параметра группировки, если оно не определено пользователем
    public readonly string DefaultGroupParameterValue = "???";
    // Допуск для геометрических операций
    public readonly double DefaultTolerance = 1e-9;
    // Размер ячейки для грубого поиска границ здания
    public readonly double DefaultCellsCoarseStepMm = 1500;
    // Размер ячейки для точного поиска границ здания
    public readonly double DefaultCellsFineStepMm = 30;
    // Размер участка, на которые разбивается длинная кривая
    public readonly double DefaultLengthSegmentMm = 200;
    // Отступ количества ячеек от границы здания
    public readonly int DefaultCellsMargin = 50;
    // Размер ячейки для пространственного индекса
     public readonly double DefaultCellsSizeForIndexMm = 10;
     // Минимальное расстояние для точек, до которого они будут соединяться в одну точку
     public readonly double DefaultMinDistanceToJoinPointsMm = 1;
     // Максимальное расстояние, до которого они будут соединяться линиями (микро разрывы)
     public readonly double DefaultMaxDistanceToCreateCurveFineMm = 20;
     // Максимальное расстояние, до которого они будут соединяться линиями (большие разрывы)
     public readonly double DefaultMaxDistanceToCreateCurveCoarseMm = 500;
    
    // Коллекция категорий, которые надо разрезать по высоте сечения (центр)
    // так как эни чаще являются полнотелыми конструкциями
    public readonly ICollection<BuiltInCategory> CenterProjectionCats = [
        BuiltInCategory.OST_Floors,
        BuiltInCategory.OST_Roofs,
        BuiltInCategory.OST_Walls,
        BuiltInCategory.OST_GenericModel
    ];
    
    // Коллекция категорий, которые надо не разрезать, а брать полную проекцию
    // так как они могут иметь прерывистую геометрию
    public readonly ICollection<BuiltInCategory> FullProjectionCats = [
        BuiltInCategory.OST_Railings,
        BuiltInCategory.OST_RailingSystem,
        BuiltInCategory.OST_StairsRailing,
        BuiltInCategory.OST_StairsRailingBaluster,
        BuiltInCategory.OST_StairsRailingRail,
        BuiltInCategory.OST_RailingBalusterRail,
        BuiltInCategory.OST_RailingBalusterRailCut,
        BuiltInCategory.OST_RailingHandRail,
        BuiltInCategory.OST_RailingHandRailAboveCut
    ];
    
    // Коллекция категорий, которые надо разрезать с отступом от низа и верха
    // для того, чтобы максимально охватить геометрию без возможных выступающих частей
    public readonly ICollection<BuiltInCategory> PartialProjectionCats = [
        BuiltInCategory.OST_Doors,
        BuiltInCategory.OST_Windows
    ];
}

