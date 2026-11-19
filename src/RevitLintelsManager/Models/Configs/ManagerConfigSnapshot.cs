using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RevitLintelsManager.Models.Configs;

/// <summary>Независимая конфигурация запуска; не содержит объектов Revit и изменяемых DTO.</summary>
internal sealed class ManagerConfigSnapshot(
    ConfigElementKey lintelFamily, IEnumerable<ConfigElementKey> lintelTypes,
    IEnumerable<ConfigElementKey> openingFamilies, IEnumerable<ConfigElementKey> structureWallTypes,
    ConfigElementKey phase, IEnumerable<KeyValuePair<string, string>> parameters,
    string ruleConfigName, IEnumerable<PlacementRuleSnapshot> rules, double minimalHeightAboveOpeningMm) {
    public ConfigElementKey LintelFamily { get; } = lintelFamily;
    public IReadOnlyList<ConfigElementKey> LintelTypes { get; } = lintelTypes.ToList().AsReadOnly();
    public IReadOnlyList<ConfigElementKey> OpeningFamilies { get; } = openingFamilies.ToList().AsReadOnly();
    public IReadOnlyList<ConfigElementKey> StructureWallTypes { get; } = structureWallTypes.ToList().AsReadOnly();
    public ConfigElementKey Phase { get; } = phase;
    public IReadOnlyDictionary<string, string> Parameters { get; } =
        new ReadOnlyDictionary<string, string>(parameters.ToDictionary(pair => pair.Key, pair => pair.Value));
    public string RuleConfigName { get; } = ruleConfigName;
    public IReadOnlyList<PlacementRuleSnapshot> Rules { get; } = rules.ToList().AsReadOnly();
    public double MinimalHeightAboveOpeningMm { get; } = minimalHeightAboveOpeningMm;
}

/// <summary>Ключ элемента конфигурации в исходном документе.</summary>
internal sealed class ConfigElementKey(string uniqueId, string name) {
    public string UniqueId { get; } = uniqueId;
    public string Name { get; } = name;
}

/// <summary>Зафиксированная группа правил с назначенными типами стен.</summary>
internal sealed class PlacementRuleSnapshot(string name, IEnumerable<ConfigElementKey> wallTypes,
    IEnumerable<LintelRuleSnapshot> rules) {
    public string Name { get; } = name;
    public IReadOnlyList<ConfigElementKey> WallTypes { get; } = wallTypes.ToList().AsReadOnly();
    public IReadOnlyList<LintelRuleSnapshot> Rules { get; } = rules.ToList().AsReadOnly();
}

/// <summary>Неизменяемый диапазон ширины и выбранный тип перемычки.</summary>
internal sealed class LintelRuleSnapshot(int minWidth, int maxWidth, int wallSupport, string lintelType) {
    public int MinWidth { get; } = minWidth;
    public int MaxWidth { get; } = maxWidth;
    public int WallSupport { get; } = wallSupport;
    public string LintelType { get; } = lintelType;
}
