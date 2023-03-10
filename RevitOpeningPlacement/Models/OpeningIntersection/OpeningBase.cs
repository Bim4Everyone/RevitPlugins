using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.Revit.Geometry;

using RevitClashDetective.Models.Extensions;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningIntersection {
    /// <summary>
    /// Базовый класс отверстия - обертка экземпляров семейств заданий на отверстия и "чистовых" отверстий
    /// </summary>
    internal class OpeningBase : ISolidProvider {
        /// <summary>
        /// Экземпляр семейства задания на отверстие или "чистового" семейства, показываемого на чертежах
        /// </summary>
        private protected readonly FamilyInstance _element;

        public OpeningBase(FamilyInstance familyInstance) {
            _element = familyInstance;
        }

        /// <summary>
        /// Возвращает Solid отверстия с трансформированными координатами
        /// </summary>
        /// <returns></returns>
        public virtual Solid GetSolid() {
            return _element.GetSolid();
        }

        /// <summary>
        /// Возвращает BoundingBoxXYZ с учетом расположения <see cref="_element">элемента</see> в файле Revit
        /// </summary>
        /// <returns></returns>
        public BoundingBoxXYZ GetTransformedBBoxXYZ() {
            return GetSolid().GetBoundingBox().TransformBoundingBox(_element.GetTotalTransform().Inverse);
        }
    }
}
