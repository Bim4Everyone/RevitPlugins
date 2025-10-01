using System;

using Autodesk.Revit.DB;

using dosymep.Revit.Geometry;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Services;
internal class SolidProviderUtils : ISolidProviderUtils {
    /// <summary>
    /// Точность для определения расстояний и координат 1 мм.
    /// </summary>
    private const double _toleranceDistance = 1 / 304.8;

    /// <summary>
    /// Точность для определения объемов 1 см3
    /// </summary>
    private const double _toleranceVolume = 10 / 304.8 * (10 / 304.8) * (10 / 304.8);

    /// <summary>
    /// Процент толерантности объемов солидов
    /// </summary>
    private const double _toleranceVolumePercentage = 0.01;


    public SolidProviderUtils() {
    }


    /// <summary>
    /// Проверяет на равенство 2 тела. <br/>
    /// Под равенством понимается равенство объемов с точностью до 1% <br/>
    /// и равенство координат боксов тел (<see cref="BoundingBoxXYZ"/>) с точностью до <paramref name="tolerance"/> в единицах длины Revit (футах).
    /// </summary>
    /// <param name="solidProvider">Первое тело.</param>
    /// <param name="otherSolid">Второе тело.</param>
    /// <param name="tolerance">Допустимое расстояние в единицах длины Revit (футах) между телами.</param>
    /// <returns>True, если разница объемов тел меньше, либо равна 1% от объема меньшего солида, <br/>
    /// и если разница координат ограничивающих боксов (<see cref="BoundingBoxXYZ"/>) меньше, либо равна <paramref name="tolerance"/>;<br/>
    /// Иначе False</returns>
    public bool EqualsSolid(ISolidProvider solidProvider, Solid otherSolid, double tolerance) {
        var thisSolid = solidProvider.GetSolid();

        if(!SolidsVolumesEqual(thisSolid, otherSolid)) {
            return false;
        }

        var thisSolidBBox = thisSolid.GetTransformedBoundingBox();
        var otherSolidBBox = otherSolid.GetTransformedBoundingBox();

        return BBoxesEqual(thisSolidBBox, otherSolidBBox, tolerance);
    }

    public bool IntersectsSolid(ISolidProvider thisSolidProvider, Solid otherSolid, BoundingBoxXYZ otherSolidBBox) {
        var thisSolid = thisSolidProvider.GetSolid();
        var thisBBox = thisSolidProvider.GetTransformedBBoxXYZ();
        return SolidsIntersect(thisSolid, thisBBox, otherSolid, otherSolidBBox);
    }


    private bool SolidsIntersect(Solid firstSolid, BoundingBoxXYZ firstBBox, Solid secondSolid, BoundingBoxXYZ secondBBox) {
        if((firstSolid is null) || (secondSolid is null)) {
            return false;
        }

        bool firstBBoxIntersectsSecond = firstBBox.IsIntersected(secondBBox);
        if(!firstBBoxIntersectsSecond) {
            return false;
        }

        // Проверка объектов на совпадение без использования BooleanOperationUtils
        if(SolidEquals(firstSolid, firstBBox, secondSolid, secondBBox)) {
            // Оставить проверку на равенство через ISolidProviderExtension.EqualsSolidProvider,
            // т.к. при солидах, смещенных друг относительно друга на [0.16-0.17] мм когда один солид внутри другого,
            // методы BooleanOperationUtils.ExecuteBooleanOperation могут выбрасывать Autodesk.Revit.Exceptions.InvalidOperationException,
            // если солиды будет смещены на 0.15 мм или меньше, то BooleanOperationUtils.ExecuteBooleanOperation не будет учитывать эту разницу в координатах,
            // при разнице 0.18 мм и больше методы BooleanOperationUtils.ExecuteBooleanOperation работают в соответствии со своими названиями операций.
            //
            // Если же солиды заходят друг в друга на расстояние, меньше 0.15953 мм, то методы BooleanOperationUtils.ExecuteBooleanOperation не будут находить пересечение.
            // При пересечении 0.15953 и более методы будут работать в соответствии с названиями своих операций.
            return true;
        }

        // Итоговая проверка на пересечение объектов
        try {
            var intersectSolid = BooleanOperationsUtils.ExecuteBooleanOperation(firstSolid, secondSolid, BooleanOperationsType.Intersect);
            if(intersectSolid?.Volume > _toleranceVolume) {
                return true;
            }
        } catch(Autodesk.Revit.Exceptions.InvalidOperationException) {
            return firstBBoxIntersectsSecond;
        }
        return false;
    }

    /// <summary>
    /// Проверяет на равенство текущего <see cref="Solid"/> и поданного <see cref="Solid"/>.
    /// <para> Под равенством понимается равенство объемов с точностью до 1% объема от меньшего солида и равенство координат ограничивающих <see cref="BoundingBoxXYZ"/> с точностью до 1 мм.</para>
    /// </summary>
    /// <param name="thisSolid">Первый Solid</param>
    /// <param name="thisSolidBBox">BoundingBox первого Solid</param> 
    /// <param name="otherSolid">Второй Solid</param>
    /// <param name="otherSolidBBox">BoundingBox второго Solid</param>
    /// <returns>True, если разница объемов текущего и поданного Solid меньше, либо равна 1 см3, 
    /// и если разница координат ограничивающих их <see cref="BoundingBoxXYZ"/> меньше, либо равна 1 мм;
    /// Иначе False</returns>
    private bool SolidEquals(Solid thisSolid, BoundingBoxXYZ thisSolidBBox, Solid otherSolid, BoundingBoxXYZ otherSolidBBox) {
        return SolidsVolumesEqual(thisSolid, otherSolid) && BBoxesEqual(thisSolidBBox, otherSolidBBox);
    }

    /// <summary>
    /// Проверяет объемы солидов на равенство.
    /// Если абсолютная разность объемов солидов не превышает 1% объема меньшего солида, возвращается True, иначе False
    /// </summary>
    /// <param name="solid1">Первый солид</param>
    /// <param name="solid2">Второй солид</param>
    /// <returns>True, если разница объемов не превышает процент объема <see cref="_toleranceVolumePercentage"/> меньшего солида</returns>
    private bool SolidsVolumesEqual(Solid solid1, Solid solid2) {
        if((solid1 is null) || (solid2 is null)) {
            return false;
        }
        double minVolume = Math.Min(solid1.Volume, solid2.Volume);
        double volumeTolerance = minVolume * _toleranceVolumePercentage;
        return Math.Abs(solid1.Volume - solid2.Volume) <= volumeTolerance;
    }

    private bool BBoxesEqual(BoundingBoxXYZ bbox1, BoundingBoxXYZ bbox2) {
        return BBoxesEqual(bbox1, bbox2, _toleranceDistance);
    }

    private bool BBoxesEqual(BoundingBoxXYZ bbox1, BoundingBoxXYZ bbox2, double tolerance) {
        double minDistance = (bbox1.Min - bbox2.Min).GetLength();
        if(minDistance > tolerance) {
            return false;
        }
        double maxDistance = (bbox1.Max - bbox2.Max).GetLength();
        return maxDistance <= tolerance;
    }
}
