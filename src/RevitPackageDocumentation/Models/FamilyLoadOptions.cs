using Autodesk.Revit.DB;

namespace RevitPackageDocumentation.Models;

/// <summary>
/// Настройки загрузки семейства в проект с заменой существующего семейства и значений его параметров
/// </summary>
internal class FamilyLoadOptions : IFamilyLoadOptions {
    public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues) {
        overwriteParameterValues = true;
        return true;
    }

    public bool OnSharedFamilyFound(
        Family sharedFamily, bool familyInUse, out FamilySource source, out bool overwriteParameterValues) {
        // Вложенные общие семейства также берем из загружаемого файла
        source = FamilySource.Family;
        overwriteParameterValues = true;
        return true;
    }
}
