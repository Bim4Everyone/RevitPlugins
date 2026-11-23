using RevitLintelsManager.Models.Settings;

namespace RevitLintelsManager.Models.Configs;

/// <summary>Результат разрешения сохранённого выбора в исходном документе.</summary>
public enum StructureTypeResolution {
    Resolved,
    Missing,
    Ambiguous,
    Invalid,
    UnsupportedCategory
}

/// <summary>Хранит исходный ключ и результат разрешения без ссылок на объекты Revit.</summary>
public sealed class StructureTypeSelection {
    /// <summary>Создаёт неизменяемый результат для одного сохранённого ключа.</summary>
    public StructureTypeSelection(StructureTypeSettings settings, StructureTypeResolution resolution,
        string uniqueId = null) {
        Settings = settings;
        Resolution = resolution;
        UniqueId = uniqueId;
    }

    public StructureTypeSettings Settings { get; }
    public StructureTypeResolution Resolution { get; }
    public string UniqueId { get; }
}
