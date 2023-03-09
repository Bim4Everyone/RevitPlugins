using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.UI;

using RevitOpeningPlacement.Models.Extensions;

namespace RevitOpeningPlacement.Models.OpeningIntersection {
    /// <summary>
    /// Тестовый класс для работы с семействами заданий на отверстия
    /// </summary>
    internal static class Test {
        internal static void TestMethod(RevitRepository revitRepository) {
            var openingsList = revitRepository.GetOpeningsTaskFromCurrentDoc();
            var opening1 = new Opening(openingsList[0]);
            var opening2 = new Opening(openingsList[1]);

            var areIntersected = opening1.IntersectsSolidProvider(opening2);
            var message = areIntersected ? "пересекаются" : "не пересекаются";
            TaskDialog.Show("Тест BooleanOperationUtils", message);
        }
    }
}
