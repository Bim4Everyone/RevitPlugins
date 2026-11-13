using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitOpeningPlacement.Services;
/// <summary>
/// Сервис поиска основы (хоста) отверстия или задания на отверстие среди элементов конструкций
/// </summary>
internal interface IHostFinder {
    /// <summary>
    /// Возвращает элемент конструкции, который наиболее похож на основу для заданного солида.
    /// <para>Под наиболее подходящим понимается элемент, с которым пересечение наибольшего объема.</para>
    /// </summary>
    /// <param name="solid">Солид отверстия или задания на отверстие
    /// в координатах документа элементов-кандидатов</param>
    /// <param name="hostCandidates">Элементы конструкций - кандидаты на основу</param>
    /// <returns>Наиболее подходящий элемент, либо null, если кандидатов нет</returns>
    Element FindBestHost(Solid solid, ICollection<Element> hostCandidates);

    /// <summary>
    /// Проверяет, относятся ли заданные элементы к разным категориям
    /// </summary>
    /// <param name="elements">Элементы конструкций</param>
    /// <returns>True, если среди элементов больше одной категории, иначе False</returns>
    bool InDifferentCategories(ICollection<Element> elements);
}
