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
    /// Класс отверстия - обертка экземпляров семейств заданий на отверстия и "чистовых" отверстий
    /// </summary>
    internal class Opening : ISolidProvider {
        private readonly FamilyInstance _element;

        public Opening(FamilyInstance familyInstance) {
            _element = familyInstance;
        }

        /// <summary>
        /// Возвращает Solid отверстия с трансформированными координатами
        /// </summary>
        /// <returns></returns>
        public Solid GetSolid() {
            return _element.GetSolid();
        }

        public BoundingBoxXYZ GetTransformedBBoxXYZ() {
            return GetSolid().GetBoundingBox().TransformBoundingBox(_element.GetTotalTransform().Inverse);
        }
    }
}
