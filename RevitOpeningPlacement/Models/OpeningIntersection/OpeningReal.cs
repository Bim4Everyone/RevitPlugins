using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

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
            return base.GetSolid();
        }
    }
}
