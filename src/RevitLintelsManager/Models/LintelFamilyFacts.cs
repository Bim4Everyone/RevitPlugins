using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RevitLintelsManager.Models;

/// <summary>Единицы значения для будущей записи параметра экземпляра.</summary>
internal enum LintelParameterKind { LengthMm, YesNo }

/// <summary>Доступность настроенного параметра для отдельного экземпляра.</summary>
internal enum LintelParameterState { Writable, Missing, Ambiguous, Incompatible, Type, ReadOnly, Unavailable }

/// <summary>Сопоставление роли параметра с настройками и независимым типом значения.</summary>
internal sealed class LintelParameterSpec(string key, LintelParameterKind kind, BearingSide? familySide = null) {
    public string Key { get; } = key;
    public LintelParameterKind Kind { get; } = kind;
    public BearingSide? FamilySide { get; } = familySide;

    public static IReadOnlyList<LintelParameterSpec> All { get; } = new List<LintelParameterSpec> {
        new(nameof(Configs.LintelFamilyConfig.LintelWidth), LintelParameterKind.LengthMm),
        new(nameof(Configs.LintelFamilyConfig.LintelThickness), LintelParameterKind.LengthMm),
        new(nameof(Configs.LintelFamilyConfig.LintelLeftOffset), LintelParameterKind.LengthMm, BearingSide.Left),
        new(nameof(Configs.LintelFamilyConfig.LintelRightOffset), LintelParameterKind.LengthMm, BearingSide.Right),
        new(nameof(Configs.LintelFamilyConfig.LintelLeftCorner), LintelParameterKind.YesNo, BearingSide.Left),
        new(nameof(Configs.LintelFamilyConfig.LintelRightCorner), LintelParameterKind.YesNo, BearingSide.Right),
        new(nameof(Configs.LintelFamilyConfig.LintelLeftWelding), LintelParameterKind.YesNo, BearingSide.Left),
        new(nameof(Configs.LintelFamilyConfig.LintelRightWelding), LintelParameterKind.YesNo, BearingSide.Right),
        new(nameof(Configs.LintelManagerConfig.LintelFixParam), LintelParameterKind.YesNo)
    }.AsReadOnly();
}

/// <summary>Результат проверки сопоставления параметра в исходном документе.</summary>
internal sealed class LintelParameterFact(string name, LintelParameterState state) {
    public string Name { get; } = name;
    public LintelParameterState State { get; } = state;
}

/// <summary>Числовые факты загруженного семейства и контекст будущего размещения.</summary>
internal sealed class LintelFamilyFacts(string familyId, IEnumerable<string> typeIds, int? familyLeftSign,
    IReadOnlyDictionary<string, LintelParameterFact> parameters, long? activeWorksetId) {
    public string FamilyId { get; } = familyId;
    public IReadOnlyList<string> TypeIds { get; } = typeIds.ToList().AsReadOnly();
    public int? FamilyLeftSign { get; } = familyLeftSign;
    public IReadOnlyDictionary<string, LintelParameterFact> Parameters { get; } =
        new ReadOnlyDictionary<string, LintelParameterFact>(
            (parameters ?? new Dictionary<string, LintelParameterFact>()).ToDictionary(p => p.Key, p => p.Value));
    public long? ActiveWorksetId { get; } = activeWorksetId;
}
