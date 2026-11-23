using System.Collections.Generic;
using System.Linq;

using pyRevitLabs.Json;
using pyRevitLabs.Json.Linq;

namespace RevitLintelsManager.Models.Settings;

public class LintelManagerSettings {
    public LintelFamilySettings LintelFamilySettings { get; set; }
    public OpeningFamilySettings OpeningFamilySettings { get; set; }
    public string LintelConfigRuleName { get; set; }
    public string LintelFixParamName { get; set; }
    public double MinimalHeightAboveOpeningMm { get; set; }
    public IEnumerable<string> StructureWallTypeNames { get; set; }
    public string PhaseName { get; set; }
    public IEnumerable<LintelWallSettings> LintelWallSettings { get; set; } = [];
    public IEnumerable<StructureTypeSettings> ReinforcedConcreteTypes { get; set; }
    public IEnumerable<StructureTypeSettings> SteelTypes { get; set; }

    [JsonIgnore]
    public double? MinimalBearingMm {
        get => _savedMinimalBearingMm?.Type switch {
            null or JTokenType.Null => null,
            JTokenType.Integer or JTokenType.Float => (double?) _savedMinimalBearingMm,
            _ => double.NaN
        };
        set => _savedMinimalBearingMm = value.HasValue ? new JValue(value.Value) : null;
    }

    // Сохраняет исходное значение JSON при сохранении других полей, даже если его тип не подходит.
    [JsonProperty(nameof(MinimalBearingMm))]
    private JToken _savedMinimalBearingMm { get; set; }

    /// <summary>Читает явный выбор ЖБ или преобразует унаследованный список стен в памяти.</summary>
    internal IEnumerable<StructureTypeSettings> GetReinforcedConcreteTypes() {
        return ReinforcedConcreteTypes ?? (StructureWallTypeNames ?? []).Select(StructureTypeSettings.FromLegacyWall);
    }

    /// <summary>Подготавливает отдельный DTO для явного сохранения из существующего редактора стен.</summary>
    internal LintelManagerSettings WithStructureWallSelection(IEnumerable<string> wallNames) {
        var updated = (LintelManagerSettings) MemberwiseClone();
        var selectedNames = wallNames.ToHashSet();
        if(ReinforcedConcreteTypes is null) {
            var previousNames = (StructureWallTypeNames ?? []).ToArray();
            var mergedNames = previousNames.Where(selectedNames.Contains)
                .Concat(selectedNames.Except(previousNames)).ToArray();
            updated.StructureWallTypeNames = mergedNames;
            updated.ReinforcedConcreteTypes = mergedNames.Select(StructureTypeSettings.FromLegacyWall).ToArray();
        } else {
            var previousWallNames = ReinforcedConcreteTypes.Where(type => type?.IsWallType() == true)
                .Select(type => type.TypeName).ToHashSet();
            if(!selectedNames.SetEquals(previousWallNames)) {
                updated.ReinforcedConcreteTypes = ReinforcedConcreteTypes
                    .Where(type => type?.IsWallType() != true || selectedNames.Contains(type.TypeName))
                    .Concat(selectedNames.Except(previousWallNames).Select(StructureTypeSettings.FromLegacyWall))
                    .ToArray();
            }
        }
        updated.SteelTypes ??= [];
        return updated;
    }
}
