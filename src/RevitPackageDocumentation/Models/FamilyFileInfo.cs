using System.Collections.Generic;

namespace RevitPackageDocumentation.Models;

/// <summary>
/// Сведения о файле семейства, полученные без загрузки семейства в проект
/// </summary>
internal class FamilyFileInfo {
    public FamilyFileInfo(string familyName, bool isGenericAnnotation, IReadOnlyList<string> typeNames) {
        FamilyName = familyName;
        IsGenericAnnotation = isGenericAnnotation;
        TypeNames = typeNames;
    }

    /// <summary>
    /// Имя семейства (имя файла без расширения)
    /// </summary>
    public string FamilyName { get; }

    /// <summary>
    /// Является ли семейство типовой аннотацией
    /// </summary>
    public bool IsGenericAnnotation { get; }

    /// <summary>
    /// Имена типоразмеров семейства
    /// </summary>
    public IReadOnlyList<string> TypeNames { get; }
}
