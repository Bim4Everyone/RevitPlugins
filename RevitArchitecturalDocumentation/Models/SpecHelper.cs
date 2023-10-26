using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitArchitecturalDocumentation.Models {
    internal class SpecHelper {
        public SpecHelper(ScheduleSheetInstance scheduleSheetInstance) {

            SpecSheetInstance = scheduleSheetInstance;
            SpecSheetInstancePoint = scheduleSheetInstance.Point;
            Specification = scheduleSheetInstance.Document.GetElement(scheduleSheetInstance.ScheduleId) as ViewSchedule;
        }

        public ScheduleSheetInstance SpecSheetInstance { get; set; }
        public XYZ SpecSheetInstancePoint { get; set; }
        public ViewSchedule Specification { get; set; }
        public List<ScheduleFilter> SpecificationFilters { get; set; }
        public List<string> SpecFilterNames { get; set; }
        public int LevelNumber { get; set; }
        public string FormatOfLevelNumber { get; set; } = string.Empty;
        public string FirstPartOfSpecName { get; set; }
        public string LastPartOfSpecName { get; set; }
        //public string PrefixOfSpecName { get; set; }
        public string SuffixOfLevelNumber { get; set; }
        public bool CanWorkWithIt { get; set; } = true;



        public List<string> GetFilterNames() {

            ScheduleDefinition scheduleDefinition = Specification.Definition;
            SpecificationFilters = scheduleDefinition.GetFilters().ToList();

            SpecFilterNames = SpecificationFilters
                .Select(o => scheduleDefinition.GetField(o.FieldId))
                .Select(o => o.GetName())
                .Distinct()
                .OrderBy(o => o)
                .ToList();

            return SpecFilterNames;
        }



        public void GetInfo() {

            // "О_ПСО_05 этаж_Жилье Корпуса 1-3"
            // [FirstPartOfSpecName][PrefixOfSpecName] NUMBER [SuffixOfSpecName][LastPartOfSpecName]
            // [О_ПСО_][0] 5 [ этаж][_Жилье Корпуса 1-3]

            if(!Specification.Name.Contains("_") || !Specification.Name.Contains("_")) {
                CanWorkWithIt= false;
                return;
            }

            // "05 этаж"
            string keyPartOfName = Specification.Name.Split('_')
                                                .FirstOrDefault(o => o.Contains("этаж"));

            if(keyPartOfName is null) {
                CanWorkWithIt = false;
                return;
            }

            // "О_ПСО_"
            FirstPartOfSpecName = Specification.Name.Replace(keyPartOfName, "`")
                                                .Split('`')[0];
            // "_Жилье Корпуса 1-3"
            LastPartOfSpecName = Specification.Name.Replace(keyPartOfName, "`")
                                    .Split('`')[1];

            // "05"
            string levelNumberAsStr = keyPartOfName.Replace(" ", "").Replace("этаж", "");


            int levelNumberAsInt;
            if(!int.TryParse(levelNumberAsStr, out levelNumberAsInt)) {
                CanWorkWithIt = false;
                return;
            }
            LevelNumber = levelNumberAsInt;


            for(int i = 0; i < levelNumberAsStr.Length; i++) { FormatOfLevelNumber += "0"; }

            FormatOfLevelNumber = "{0:" + FormatOfLevelNumber + "}";

            //PrefixOfSpecName = levelNumberAsStr.Replace(String.Format(FormatOfLevelNumber, levelNumberAsInt), "");
            SuffixOfLevelNumber = keyPartOfName.Replace(levelNumberAsStr, "");
        }
    }
}
