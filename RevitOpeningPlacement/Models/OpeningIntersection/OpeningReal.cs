using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using DevExpress.DirectX.Common.Direct2D;

using dosymep.Revit;
using dosymep.Revit.Geometry;

using RevitClashDetective.Models.Extensions;

using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningIntersection {
    /// <summary>
    /// Класс - обертка "чистовых" экземпляров семейств, обычно полых, 
    /// которые проектировщик размещает в местах расположения экземпляров семейств <seealso cref="OpeningTask">заданий на отверстия</seealso>
    /// </summary>
    internal class OpeningReal : ISolidProvider {
        /// <summary>
        /// Экземпляр семейства отверстия, которое идет в чертежи
        /// </summary>
        private readonly FamilyInstance _element;

        public OpeningReal(FamilyInstance familyInstance) {
            _element = familyInstance;
        }

        /// <summary>
        /// Получение Solid "чистового" отверстия с полой геометрией
        /// </summary>
        /// <returns></returns>
        public Solid GetSolid() {
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
        /// Возвращает BoundingBoxXYZ с учетом расположения <see cref="_element">элемента</see> в файле Revit
        /// </summary>
        /// <returns></returns>
        public BoundingBoxXYZ GetTransformedBBoxXYZ() {
            return _element.GetBoundingBox().TransformBoundingBox(_element.GetTotalTransform().Inverse);
        }

        /// <summary>
        /// Возвращает хост экземпляра семейства <see cref="OpeningTask._element">отверстия</see>
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public Element GetHost() {
            var host = _element.Host;
            if(host is null) {
                throw new NullReferenceException($"Хост элемента с Id: {_element.Id} - null");
            }
            return host;
        }
    }
}
