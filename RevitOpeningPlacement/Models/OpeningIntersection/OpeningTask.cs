using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitOpeningPlacement.Models.OpeningIntersection {

    /// <summary>
    /// Класс - обертка экземпляров семейств заданий на отверстия
    /// </summary>
    internal class OpeningTask : OpeningBase {
        public OpeningTask(FamilyInstance familyInstance) : base(familyInstance) {
        }
    }
}
