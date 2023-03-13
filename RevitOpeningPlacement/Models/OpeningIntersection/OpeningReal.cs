using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Revit.Geometry;

using RevitClashDetective.Models.Extensions;

using RevitOpeningPlacement.Models.Extensions;

namespace RevitOpeningPlacement.Models.OpeningIntersection {
    /// <summary>
    /// Класс - обертка "чистовых" экземпляров семейств, которые проектировщик размещает в местах расположений экземпляров семейств заданий на отверстия
    /// </summary>
    internal class OpeningReal : OpeningBase {
        public OpeningReal(FamilyInstance familyInstance) : base(familyInstance) {
        }

        /// <summary>
        /// Получение Solid "чистового" отверстия с полой геометрией
        /// </summary>
        /// <returns></returns>
        public override Solid GetSolid() {
            XYZ openingLocation = (_element.Location as LocationPoint).Point;
            var hostElement = GetHost();
            Solid hostSolidCut = hostElement.GetSolid();
            Solid hostSolidOriginal = (hostElement as HostObject).GetHostElementOriginalSolid();
            var openings = SolidUtils.SplitVolumes(BooleanOperationsUtils.ExecuteBooleanOperation(hostSolidOriginal, hostSolidCut, BooleanOperationsType.Difference));
            var thisOpeningSolid = openings.OrderBy(solidOpening => (solidOpening.ComputeCentroid() - openingLocation).GetLength()).FirstOrDefault();
            if(thisOpeningSolid != null) {
                return thisOpeningSolid;
            } else {
                return GetTransformedBBoxXYZ().CreateSolid();
            }
        }

        /// <summary>
        /// Возвращает хост экземпляра семейства <see cref="OpeningBase._element">отверстия</see>
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        private Element GetHost() {
            var host = _element.Host;
            if(host is null) {
                throw new NullReferenceException($"Host of element with id: {_element.Id} returned null");
            }
            return host;
        }
    }
}
