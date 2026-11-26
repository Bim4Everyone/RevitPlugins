using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;

using pyRevitLabs.Json;

using RevitLintelsManager.Models.Settings;

namespace RevitLintelsManager.Models.Configs;

/// <summary>
/// Класс конфигурации плагина.
/// </summary>
internal class PluginConfig : ProjectConfig<RevitSettings> {
    [JsonIgnore]
    public override string ProjectConfigPath { get; set; }
    
    [JsonIgnore]
    public override IConfigSerializer Serializer { get; set; }
    
    public static PluginConfig GetPluginConfig(IConfigSerializer configSerializer) {
        return new ProjectConfigBuilder()
            .SetSerializer(configSerializer)
            .SetPluginName(nameof(RevitLintelsManager))
            .SetRevitVersion(ModuleEnvironment.RevitVersion)
            .SetProjectConfigName(nameof(PluginConfig) + ".json")
            .Build<PluginConfig>();
    }
}

internal class RevitSettings : ProjectSettings {
    
    public override string ProjectName { get; set; }
    
    public LintelManagerSettings LintelManagerSettings { get; set; }
}

public class SystemPluginConfig {
    internal const string BatchHostResource = nameof(RevitLintelsManager) + ".UI.exe";
    internal const string BatchCancelButton = "_cancelButton";
    internal const string BatchProgressProvider = "dosymep.Xpf.Core.SimpleServices.XtraProgressDialogService, dosymep.Xpf.Core";
    internal const string BatchProgressThemeProperty = "UIThemeService";
    internal const string BatchReady = "Ready";
    internal const string BatchCancel = "Cancel";
    internal const int BatchReleaseOffset = sizeof(int);
    internal const int BatchChannelSize = BatchReleaseOffset + sizeof(int);
    internal const int BatchRefreshMilliseconds = 40;
    internal const int BatchStartupTimeoutSeconds = 30;
    internal const string CustomGroupExpanderPartName = "PART_Expander";
    internal const double DefaultMatchAlignmentMm = 10;
    internal const double DefaultMatchSearchMm = 200;
    internal const double MatchAngleToleranceDegrees = 0.1;
    internal const double CalculationPositionToleranceMm = 1;
    internal const double CalculationLengthToleranceMm = 1;
    // Несколько ULP учитывают вычитание мировых координат и обратное преобразование единиц.
    internal const double CoordinateRoundoffFactor = 8 * 2.2204460492503131e-16;
    internal const double MillimetersPerFoot = 304.8;
    internal const double GeometryLengthEpsilonFeet = 0.001 / MillimetersPerFoot;
    // Предельный зазор совпадающих центральных плоскостей перед NewAlignment (в футах).
    internal const double LockCoincidenceToleranceFeet = 1e-9;

    // Компенсация округления при сравнении отклонения с включительной границей.
    internal const double MatchRoundoffMm = 1e-8;
    internal const double MatchRoundoffDegrees = 1e-10;

    // Коллекция категорий допустимых семейств перемычек
    public readonly IEnumerable<BuiltInCategory> DefaultLintelCats = [
        BuiltInCategory.OST_GenericModel
    ];
    
    // Коллекция категорий допустимых семейств, над которыми могут устанавливаться перемычки
    public readonly IEnumerable<BuiltInCategory> DefaultOpeningCats = [
        BuiltInCategory.OST_Doors,    
        BuiltInCategory.OST_Windows
    ];

    public IReadOnlyList<BuiltInCategory> ReinforcedConcreteCategories { get; } = Array.AsReadOnly(new[] {
        BuiltInCategory.OST_Walls,
        BuiltInCategory.OST_Floors,
        BuiltInCategory.OST_StructuralFraming,
        BuiltInCategory.OST_StructuralColumns,
        BuiltInCategory.OST_StructuralFoundation
    });

    public IReadOnlyList<BuiltInCategory> SteelCategories { get; } = Array.AsReadOnly(new[] {
        BuiltInCategory.OST_StructuralFraming,
        BuiltInCategory.OST_StructuralColumns
    });

    public double DefaultMinimalBearingMm => 70;
    public string DefaultLintelFamilyName  => "ОбщМд_Перемычка";
    public string LintelWidthParamName  => "Ширина проема";
    public string LintelThicknessParamName  => "Толщина стены";
    public string LintelRightOffsetParamName  => "Опирание справа";
    public string LintelLeftOffsetParamName  => "Опирание слева";
    public string LintelRightCornerParamName  => "Уголок справа";
    public string LintelLeftCornerParamName  => "Уголок слева";
    public string LintelRightWeldingParamName  => "Крепление на металл справа";
    public string LintelLeftWeldingParamName  => "Крепление на металл слева";
    public IEnumerable<string> DefaultOpeningFamilyNames  => [
        "Двр_Двр_Однп", 
        "Двр_Двр_Двуп",
        "Двр_Про_Проем",
        "Двр_Люк_Ревизия",
        "Окн_Окн_1 Ств Бизнес",
        "Окн_Отв_Прямоуг_Стена"
    ];
    public string OpeningHeightParamName  => LabelUtils.GetLabelFor(BuiltInParameter.INSTANCE_HEAD_HEIGHT_PARAM);
    public string OpeningWidthParamName  => LabelUtils.GetLabelFor(BuiltInParameter.FAMILY_WIDTH_PARAM);
    public string DefaultLintelConfigRuleName  => "A101";
    public string DefaultLintelFixParamName  => "ФОП_Фиксация решения";
    public double DefaultMinimalHeightAboveLintelMm  => 100;
    public string DefaultPhaseName  => "Основная планировка";
    public string DefaultRuleFolderName  => "Rules";
    public string DefaultSettingFolderName  => "dosymep";
    public string DefaultSettingPath => @"W:\Проектный институт\Отд.стандарт.BIM и RD\BIM-Ресурсы\5-Надстройки\Bim4Everyone\A101\";
    public double DefaultMinWidthMm => 400;
    public double DefaultMaxWidthMm => 900;
    public double DefaultWallSupportMm => 250;
    public double NavigationSectionBoxMarginMm => 300;
    public BuiltInCategory ManagerViewSectionBoxCategory => BuiltInCategory.OST_SectionBox;
    public BuiltInCategory ManagerViewWallCategory => BuiltInCategory.OST_Walls;
    public int ManagerViewWallTransparency => 40;
    public DisplayStyle ManagerViewDisplayStyle => DisplayStyle.Shading;
    public ViewDetailLevel ManagerViewDetailLevel => ViewDetailLevel.Fine;
    public string DefaultLintelType => String.Empty;

    
}
