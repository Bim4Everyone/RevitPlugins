using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitPackageDocumentation.Models;

/// <summary>
/// Сведения о файле семейства, полученные без загрузки семейства в проект
/// </summary>
internal class FamilyFileInfo {
    public FamilyFileInfo(string familyName, BuiltInCategory? builtInCategory, IReadOnlyList<string> typeNames) {
        FamilyName = familyName;
        FamilyCategory = builtInCategory;
        TypeNames = typeNames;
    }

    /// <summary>
    /// Имя семейства (имя файла без расширения)
    /// </summary>
    public string FamilyName { get; }

    /// <summary>
    /// Категория семейства. Null, если категорию не удалось определить
    /// </summary>
    public BuiltInCategory? FamilyCategory { get; }

    /// <summary>
    /// Имена типоразмеров семейства
    /// </summary>
    public IReadOnlyList<string> TypeNames { get; }
}
