using Autodesk.Revit.DB;

namespace RevitOpeningPlacement.Models.Interfaces {
    /// <summary>
    /// Интерфейс для чистовых проемов в файле АР или КР
    /// </summary>
    internal interface IOpeningReal : ISolidProvider, IFamilyInstanceProvider {
        /// <summary>
        /// Возвращает хост элемент проема
        /// </summary>
        /// <returns></returns>
        Element GetHost();

        /// <summary>
        /// Id экземпляра семейства чистового проема
        /// </summary>
        ElementId Id { get; }
    }
}
