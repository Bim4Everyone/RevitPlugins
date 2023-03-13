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
    /// Класс отверстия - обертка экземпляров семейств заданий на отверстия
    /// </summary>
    internal class OpeningTask : ISolidProvider {
        /// <summary>
        /// Экземпляр семейства задания на отверстие
        /// </summary>
        private readonly FamilyInstance _element;

        public OpeningTask(FamilyInstance familyInstance) {
            _element = familyInstance;
        }

        /// <summary>
        /// Возвращает Solid отверстия с трансформированными координатами
        /// </summary>
        /// <returns></returns>
        public Solid GetSolid() {
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
