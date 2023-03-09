using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Revit.Geometry;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.Extensions {
    /// <summary>
    /// Методы расширения для классов, реализующих интерфейс <see cref="ISolidProvider">ISolidProvider</see>
    /// </summary>
    internal static class ISolidProviderExtension {
        /// <summary>
        /// Точность для определения расстояний и координат 1 мм.
        /// </summary>
        private static double _toleranceDistance => 1 / 304.8;

        /// <summary>
        /// Точность для определения объемов 1 см3
        /// </summary>
        private static double _toleranceVolume => (10 / 304.8) * (10 / 304.8) * (10 / 304.8);


        /// <summary>
        /// Проверяет, пересекается ли текущий <see cref="ISolidProvider">ISolidProvider</see> с каким-либо <see cref="ISolidProvider">ISolidProvider</see> из поданного списка
        /// </summary>
        /// <param name="thisSolidProvider">Текущий <see cref="ISolidProvider">ISolidProvider</see></param>
        /// <param name="solidProviders">Поданная коллекция <see cref="ISolidProvider">ISolidProvider</see></param>
        /// <returns>True, если текущий <see cref="ISolidProvider">ISolidProvider</see> пересекается с каким-либо <see cref="ISolidProvider">ISolidProvider</see> из поданной коллекции, иначе False</returns>
        internal static bool IntersectsAnySolidProviders(this ISolidProvider thisSolidProvider, ICollection<ISolidProvider> solidProviders) {
            foreach(var solidProvider in solidProviders) {
                if(thisSolidProvider.IntersectsSolidProvider(solidProvider)) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Проверяет, пересекается ли текущий <see cref="ISolidProvider">ISolidProvider</see> с поданным <see cref="ISolidProvider">ISolidProvider</see>.
        /// </summary>
        /// <param name="thisSolidProvider">Текущий <see cref="ISolidProvider">ISolidProvider</see></param>
        /// <param name="solidProvider">Поданный <see cref="ISolidProvider">ISolidProvider</see></param>
        /// <returns>True, если текущий <see cref="ISolidProvider">ISolidProvider</see> пересекается с поданным <see cref="ISolidProvider">ISolidProvider</see>, иначе False.
        /// Если объекты касаются друг друга, то также False</returns>
        internal static bool IntersectsSolidProvider(this ISolidProvider thisSolidProvider, ISolidProvider solidProvider) {
            var thisSolid = thisSolidProvider.GetSolid();
            var otherSolid = solidProvider.GetSolid();

            // первичная проверка на пересечение BoundingBoxXYZ
            var thisBBox = thisSolidProvider.GetTransformedBBoxXYZ();
            var otherBBox = solidProvider.GetTransformedBBoxXYZ();
            bool thisBBoxIntersectsOther = thisBBox.IsIntersected(otherBBox);
            if(!thisBBoxIntersectsOther) {
                return false;
            }

            // Проверка объектов на совпадение без использования BooleanOperationUtils
            if(thisSolidProvider.EqualsSolidProvider(solidProvider)) {
                // Оставить проверку на равенство через ISolidProviderExtension.EqualsSolidProvider,
                // т.к. при солидах, смещенных друг относительно друга на [0.16-0.17] мм,
                // методы BooleanOperationUtils.ExecuteBooleanOperation могут выбрасывать Autodesk.Revit.Exceptions.InvalidOperationException,
                // если солиды будет смещены на 0.15 мм или меньше, то BooleanOperationUtils.ExecuteBooleanOperation не будет учитывать эту разницу в координатах,
                // при разнице 0.18 мм и больше методы BooleanOperationUtils.ExecuteBooleanOperation работают в соответствии со своими названиями операций.
                //
                // Если же солиды заходят друг в друга на расстояние, меньше 0.15953 мм, то то методы BooleanOperationUtils.ExecuteBooleanOperation не будут находить пересечение
                // при пересечении 0.15953 и более методы будут работать в соответствии с названиями своих операций.
                return true;
            }

            // Итоговая проверка на пересечение объектов
            Solid intersectSolid = null;
            try {
                intersectSolid = BooleanOperationsUtils.ExecuteBooleanOperation(thisSolid, otherSolid, BooleanOperationsType.Intersect);
                if(intersectSolid.Volume > _toleranceVolume) {
                    return true;
                }
            } catch(Autodesk.Revit.Exceptions.InvalidOperationException) {
                return thisBBoxIntersectsOther;
            }
            return false;
        }

        /// <summary>
        /// Проверяет на равенство текущего <see cref="ISolidProvider">ISolidProvider</see> и поданного <see cref="ISolidProvider">ISolidProvider</see>.
        /// Под равенством понимается равенство объемов с точностью до 1 см3 и равенство координат ограничивающих <see cref="BoundingBoxXYZ"/> с точностью до 1 мм.
        /// </summary>
        /// <param name="solidProvider">Текущий <see cref="ISolidProvider">ISolidProvider</see></param>
        /// <param name="otherSolidProvider">Поданный <see cref="ISolidProvider">ISolidProvider</see></param>
        /// <returns>True, если разница объемов текущего и поданного <see cref="ISolidProvider">ISolidProvider</see> меньше, либо равна 1 см3, 
        /// и если разница координат ограничивающих их <see cref="BoundingBoxXYZ"/> меньше, либо равна 1 мм;
        /// Иначе False</returns>
        internal static bool EqualsSolidProvider(this ISolidProvider solidProvider, ISolidProvider otherSolidProvider) {
            var thisSolid = solidProvider.GetSolid();
            var otherSolid = otherSolidProvider.GetSolid();

            if(Math.Abs(thisSolid.Volume - otherSolid.Volume) > _toleranceVolume) {
                return false;
            }

            var thisSolidBBox = solidProvider.GetTransformedBBoxXYZ();
            var otherSolidBBox = otherSolidProvider.GetTransformedBBoxXYZ();

            var minDistance = (thisSolidBBox.Min - otherSolidBBox.Min).GetLength();
            if(minDistance > _toleranceDistance) {
                return false;
            }
            var maxDistance = (thisSolidBBox.Max - otherSolidBBox.Max).GetLength();
            if(maxDistance > _toleranceDistance) {
                return false;
            }
            return true;
        }
    }
}
