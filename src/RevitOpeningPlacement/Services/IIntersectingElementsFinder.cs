using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitOpeningPlacement.Services;
/// <summary>
/// Сервис поиска элементов, пересекающихся с заданным солидом
/// </summary>
internal interface IIntersectingElementsFinder {
    /// <summary>
    /// Возвращает Id элементов из заданного набора, которые пересекаются с заданным солидом
    /// </summary>
    /// <param name="document">Документ, в котором находятся элементы и солид</param>
    /// <param name="elementIds">Набор Id элементов, среди которых происходит поиск</param>
    /// <param name="solid">Солид в координатах <paramref name="document"/></param>
    ICollection<ElementId> GetIntersectingElementIds(
        Document document,
        ICollection<ElementId> elementIds,
        Solid solid);

    /// <summary>
    /// Возвращает Id элементов заданных категорий, которые пересекаются с заданным солидом
    /// </summary>
    /// <param name="document">Документ, в котором находятся элементы и солид</param>
    /// <param name="categories">Категории элементов, среди которых происходит поиск</param>
    /// <param name="solid">Солид в координатах <paramref name="document"/></param>
    ICollection<ElementId> GetIntersectingElementIds(
        Document document,
        ICollection<BuiltInCategory> categories,
        Solid solid);
}
