using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;

namespace RevitCopyScheduleInstance.Models {
    class SheetHelper {
        public SheetHelper(ViewSheet viewSheet) {
            Sheet = viewSheet;
        }

        public ViewSheet Sheet { get; set; }

        public int NumberOfLevel { get; set; }
        public bool HasProblemWithLevelName { get; set; } = true;

        /// <summary>
        /// Получает номер этажа по имени листа. Указывает через HasProblemWithLevelName, получилось ли это сделать
        /// </summary>
        public void GetNumberOfLevel() {

            // Предполагаем, что лист назван: "ПСО_корпус 1_секция 1_этаж 4", т.е. блок с этажом отделен "_"
            // ищем "05 этаж"
            if(!Sheet.Name.Contains("_")) { return; }
            string keyPartOfName = Sheet.Name.ToLower().Split('_').FirstOrDefault(o => o.Contains("этаж"));

            if(keyPartOfName is null) { return; }

            // Получаем "05"
            string levelNumberAsStr = keyPartOfName.Replace(" ", "").Replace("этаж", "");

            // Получаем int(5)
            if(!int.TryParse(levelNumberAsStr, out int numberOfLevelAsInt)) { return; }
            NumberOfLevel = numberOfLevelAsInt;

            HasProblemWithLevelName = false;
        }
    }
}
