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
            SpecificationDefinition = Specification.Definition;
            SpecificationFilters = SpecificationDefinition.GetFilters().ToList();
        }

        public SpecHelper(ViewSchedule viewSchedule) {

            Specification = viewSchedule;
            SpecificationDefinition = Specification.Definition;
            SpecificationFilters = SpecificationDefinition.GetFilters().ToList();
        }

        public ScheduleSheetInstance SpecSheetInstance { get; set; }
        public XYZ SpecSheetInstancePoint { get; set; }
        public ViewSchedule Specification { get; set; }
        public ScheduleDefinition SpecificationDefinition { get; set; }
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

            SpecFilterNames = SpecificationFilters
                .Select(o => SpecificationDefinition.GetField(o.FieldId))
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

                //TaskDialog.Show("fd", "1");
                CanWorkWithIt= false;
                return;
            }

            // "05 этаж"
            string keyPartOfName = Specification.Name.Split('_')
                                                .FirstOrDefault(o => o.Contains("этаж"));

            if(keyPartOfName is null) {
                //TaskDialog.Show("fd", "2");

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
                //TaskDialog.Show("fd", levelNumberAsStr);

                CanWorkWithIt = false;
                return;
            }
            LevelNumber = levelNumberAsInt;

            FormatOfLevelNumber = GetStringFormatOrDefault(levelNumberAsStr);

            SuffixOfLevelNumber = keyPartOfName.Replace(levelNumberAsStr, "");
        }

        /// <summary>
        /// Получает строку формата на основе количества символов подаваемой строки с числом
        /// Строка формата представляет собой последовательность "{0:" + "0"*{Длина входной строки} + "}"
        /// </summary>
        public string GetStringFormatOrDefault(string numAsString) {
            string format = string.Empty;

            int test;
            if(!int.TryParse(numAsString, out test)) {
                return "{0:0}";
            }

            for(int i = 0; i < numAsString.Length; i++) { format += "0"; }
            return "{0:" + format + "}";
        }


        public void ChangeSpecFilters(string specFilterName, int newFilterValue) {

            // В дальнейшем нужно предусмотреть проверки, что поле фильрации принимает строки + сеттеры для других типов

            List<ScheduleFilter> newScheduleFilters = new List<ScheduleFilter>();

            for(int i = 0; i < SpecificationFilters.Count; i++) {

                ScheduleFilter currentFilter = SpecificationFilters[i]; 
                
                ScheduleField scheduleFieldFromFilter = SpecificationDefinition.GetField(currentFilter.FieldId);

                //TaskDialog.Show("Поле фильтра", scheduleFieldFromFilter.GetName());

                if(scheduleFieldFromFilter.GetName() == specFilterName) {


                    string filterOldValue = currentFilter.GetStringValue();
                    //TaskDialog.Show("filterOldValue", filterOldValue);

                    string format = GetStringFormatOrDefault(filterOldValue);
                    //TaskDialog.Show("format", format);

                    string newVal = String.Format(format, newFilterValue);
                    //TaskDialog.Show("newVal", newVal);


                    currentFilter.SetValue(newVal);
                    newScheduleFilters.Add(currentFilter);
                } else {
                    newScheduleFilters.Add(currentFilter);
                }
            }

            SpecificationDefinition.SetFilters(newScheduleFilters);
        }
    }
}
