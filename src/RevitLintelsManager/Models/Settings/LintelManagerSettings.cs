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

    /// <summary>Подготавливает отдельный DTO для сохранения общих настроек, включая исходные legacy-данные.</summary>
    internal LintelManagerSettings WithGeneralSettings(double minimalBearingMm,
        IEnumerable<StructureTypeSettings> concreteTypes, IEnumerable<StructureTypeSettings> steelTypes) {
        var updated = (LintelManagerSettings) MemberwiseClone();
        updated.MinimalBearingMm = minimalBearingMm;
        updated.ReinforcedConcreteTypes = concreteTypes.ToArray();
        updated.SteelTypes = steelTypes.ToArray();
        return updated;
    }
}
