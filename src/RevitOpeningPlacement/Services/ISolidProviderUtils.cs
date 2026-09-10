using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Services;
internal interface ISolidProviderUtils {
    /// <summary>
    /// Проверяет на равенство 2 тела. <br/>
    /// Под равенством понимается приближенное равенство объемов и приближенное равенство координат.<br/>
    /// </summary>
    /// <param name="solidProvider">Первое тело.</param>
    /// <param name="otherSolid">Второе тело.</param>
    /// <param name="tolerance">Погрешность для определения равенства координат в единицах длины Revit (футах).</param>
    /// <returns>True, если разница объемов тел меньше, либо равна 1% от объема меньшего солида, <br/>
    /// и если разница координат меньше, либо равна <paramref name="tolerance"/>;<br/>
    /// Иначе False</returns>
    bool EqualsSolid(ISolidProvider solidProvider, Solid otherSolid, double tolerance);

    /// <summary>
    /// Метод проверяет тела на пересечение.
    /// <para>
    /// Если геометрию <paramref name="solidProvider"/> построить не удалось, возвращается False.
    /// </para>
    /// </summary>
    /// <param name="solidProvider">Первое тело.</param>
    /// <param name="otherSolid">Второе тело.</param>
    /// <param name="otherSolidBBox">Бокс второго тела.</param>
    /// <returns>True, если тела пересекаются, иначе False</returns>
    bool IntersectsSolid(ISolidProvider solidProvider, Solid otherSolid, BoundingBoxXYZ otherSolidBBox);

    /// <summary>
    /// Вычитает из заданного тела коллекцию тел.
    /// <para>Тела, которые вычесть не удалось, пропускаются.</para>
    /// </summary>
    /// <param name="source">Исходное тело.</param>
    /// <param name="solidsToSubtract">Тела, которые надо вычесть из исходного.</param>
    /// <returns>Тело, оставшееся после вычитания.</returns>
    Solid SubtractSolids(Solid source, ICollection<Solid> solidsToSubtract);
}
