using Autodesk.Revit.DB;

namespace RevitOpeningPlacement.Models.Interfaces {
    /// <summary>
    /// Интерфейс предоставляющий метод получения экземпляра семейства
    /// </summary>
    internal interface IFamilyInstanceProvider {
        /// <summary>
        /// Возвращает экземпляр семейства
        /// </summary>
        /// <returns></returns>
        FamilyInstance GetFamilyInstance();
    }
}
