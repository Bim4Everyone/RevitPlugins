using pyRevitLabs.Json;

namespace RevitLintelsManager.Models.Settings;

/// <summary>Переносимый ключ типа конструкции без идентификатора элемента другого документа.</summary>
public sealed class StructureTypeSettings {
    private const string _wallCategory = "OST_Walls";

    /// <summary>Создаёт ключ из кода категории и точных имён семейства и типа.</summary>
    [JsonConstructor]
    public StructureTypeSettings(string category, string familyName, string typeName) {
        Category = category;
        FamilyName = familyName;
        TypeName = typeName;
    }

    public string Category { get; }
    public string FamilyName { get; }
    public string TypeName { get; }

    /// <summary>Создаёт ключ системного WallType из имени в старых настройках.</summary>
    internal static StructureTypeSettings FromLegacyWall(string typeName) {
        return new StructureTypeSettings(_wallCategory, null, typeName);
    }

    /// <summary>Возвращает составной ключ для точного сравнения.</summary>
    internal (string Category, string FamilyName, string TypeName) GetKey() {
        return (Category, FamilyName, TypeName);
    }

    /// <summary>Проверяет, что позиция относится к системному типу стены.</summary>
    internal bool IsWallType() {
        return Category == _wallCategory && FamilyName is null;
    }
}
