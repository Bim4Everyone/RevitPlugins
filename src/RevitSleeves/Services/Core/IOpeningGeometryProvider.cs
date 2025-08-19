using Autodesk.Revit.DB;

namespace RevitSleeves.Services.Core;
internal interface IOpeningGeometryProvider {
    /// <summary>
    /// Находит солид отверстия в координатах собственного документа
    /// </summary>
    /// <param name="opening">Чистовое отверстие из АР/КР</param>
    /// <returns>Солид в координатах собственного документа</returns>
    Solid GetSolid(FamilyInstance opening);
}
