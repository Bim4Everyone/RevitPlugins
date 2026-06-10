using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;

using pyRevitLabs.Json;

namespace RevitPylonLoadAreas.Models;

internal sealed class SystemConfig : ProjectConfig {
    [JsonIgnore]
    public override string ProjectConfigPath { get; set; }

    [JsonIgnore]
    public override IConfigSerializer Serializer { get; set; }

    /// <summary>
    /// Шаг тесселляции в мм для линий стен для построения диаграммы Вороного
    /// </summary>
    public double WallsTessellationStepMm { get; set; } = 300;

    /// <summary>
    /// Минимальная площадь отверстий в перекрытии в м2, меньше которой отверстия не учитываются для определения грузовой площади
    /// </summary>
    public double OpeningMinAreaM2 { get; set; } = 1;

    /// <summary>
    /// Возвращает <see cref="OpeningMinAreaM2"/> в единицах Revit
    /// </summary>
    public double GetOpeningMinArea() {
        return UnitUtils.ConvertToInternalUnits(OpeningMinAreaM2, UnitTypeId.SquareMeters);
    }

    /// <summary>
    /// Возвращает <see cref="WallsTessellationStepMm"/> в единицах Revit
    /// </summary>
    public double GetWallsTessellationStep() {
        return UnitUtils.ConvertToInternalUnits(WallsTessellationStepMm, UnitTypeId.Millimeters);
    }

    public static SystemConfig GetConfig(IConfigSerializer configSerializer) {
        return new ProjectConfigBuilder()
            .SetSerializer(configSerializer)
            .SetPluginName(nameof(RevitPylonLoadAreas))
            .SetRevitVersion(ModuleEnvironment.RevitVersion)
            .SetProjectConfigName(nameof(SystemConfig) + ".json")
            .Build<SystemConfig>();
    }
}
