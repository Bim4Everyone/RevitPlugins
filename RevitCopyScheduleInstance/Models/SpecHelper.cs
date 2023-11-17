using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using RevitCopyScheduleInstance.Models;

using RevitCopyScheduleInstance.ViewModels;

namespace RevitCopyScheduleInstance.Models {
    internal class SpecHelper {
        public SpecHelper(RevitRepository revitRepository, ScheduleSheetInstance scheduleSheetInstance) {

            Repository = revitRepository;
            SpecSheetInstance = scheduleSheetInstance;
            SpecSheetInstancePoint = scheduleSheetInstance.Point;
            Specification = scheduleSheetInstance.Document.GetElement(scheduleSheetInstance.ScheduleId) as ViewSchedule;
            SpecificationDefinition = Specification.Definition;
            SpecificationFilters = SpecificationDefinition.GetFilters().ToList();
        }

        public SpecHelper(RevitRepository revitRepository, ViewSchedule viewSchedule) {

            Repository = revitRepository;
            Specification = viewSchedule;
            SpecificationDefinition = Specification.Definition;
            SpecificationFilters = SpecificationDefinition.GetFilters().ToList();
        }


        RevitRepository Repository { get; set; }
        public ViewSchedule Specification { get; set; }
        public ScheduleSheetInstance SpecSheetInstance { get; set; }
        public XYZ SpecSheetInstancePoint { get; set; }
        public ScheduleDefinition SpecificationDefinition { get; set; }
        public List<ScheduleFilter> SpecificationFilters { get; set; }
        public List<string> SpecFilterNames { get; set; }
        public int LevelNumber { get; set; }
        public bool HasProblemWithLevelDetection { get; set; } = true;
        public string FormatOfLevelNumber { get; set; } = string.Empty;
        public string FirstPartOfSpecName { get; set; }
        public string LastPartOfSpecName { get; set; }
        public string SuffixOfLevelNumber { get; set; }


        /// <summary>
        /// Метод предназначенный для формирования списка имен полей фильтров, которые есть одновременно во всех выбранных спеках
        /// </summary>
        public List<string> GetFilterNames() {

            SpecFilterNames = SpecificationFilters
                .Select(o => SpecificationDefinition.GetField(o.FieldId))
                .Select(o => o.GetName())
                .Distinct()
                .OrderBy(o => o)
                .ToList();

            return SpecFilterNames;
        }


        /// <summary>
        /// Получает информацию об имени спецификации
        /// </summary>
        public void GetNameInfo() {

            // "О_ПСО_05 этаж_Жилье Корпуса 1-3"
            // [FirstPartOfSpecName] NUMBER [SuffixOfSpecName][LastPartOfSpecName]
            // [О_ПСО_] 05 [ этаж][_Жилье Корпуса 1-3]

            if(!Specification.Name.Contains("_")) {

                HasProblemWithLevelDetection = false;
                return;
            }

            // ищем "05 этаж"
            string keyPartOfName = Specification.Name.ToLower().Split('_').FirstOrDefault(o => o.Contains("этаж"));

            if(keyPartOfName is null) {

                HasProblemWithLevelDetection = false;
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


            if(!int.TryParse(levelNumberAsStr, out int levelNumberAsInt)) {

                HasProblemWithLevelDetection = false;
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

            if(!int.TryParse(numAsString, out int test)) {
                return "{0:0}";
            }

            for(int i = 0; i < numAsString.Length; i++) { format += "0"; }
            return "{0:" + format + "}";
        }


        /// <summary>
        /// Метод находит в проекте, а если не нашел, то создает спецификацию с указанным именем и задает ей фильтрацию
        /// </summary>
        public SpecHelper GetOrDublicateNSetSpec(string filterName, int numberOfLevelAsInt) {

            string specName = FirstPartOfSpecName
                              + String.Format(FormatOfLevelNumber, numberOfLevelAsInt)
                              + SuffixOfLevelNumber
                              + LastPartOfSpecName;

            ViewSchedule newViewSpec = Repository.GetSpecByName(specName);
            SpecHelper newSpec;
            if(newViewSpec is null) {
                newViewSpec = Repository.Document.GetElement(Specification.Duplicate(ViewDuplicateOption.Duplicate)) as ViewSchedule;
                newViewSpec.Name = specName;

                newSpec = new SpecHelper(Repository, newViewSpec);
                newSpec.ChangeSpecFilters(filterName, numberOfLevelAsInt);
            } else {
                newSpec = new SpecHelper(Repository, newViewSpec);
            }
            newSpec.SpecSheetInstancePoint = SpecSheetInstancePoint;

            return newSpec;
        }


        /// <summary>
        /// Изменяет фильтры спецификации так, чтобы в фильтре поля этажа задать соответсвующий номер этажа
        /// </summary>
        public void ChangeSpecFilters(string specFilterName, int newFilterValue) {

            List<ScheduleFilter> newScheduleFilters = new List<ScheduleFilter>();

            for(int i = 0; i < SpecificationFilters.Count; i++) {

                ScheduleFilter currentFilter = SpecificationFilters[i];

                ScheduleField scheduleFieldFromFilter = SpecificationDefinition.GetField(currentFilter.FieldId);

                // Если поле фильтра не имеет имя, которое запрашивает пользователь, то фильтр добавляется без изменений
                // Предпринимались многочисленные попытки просто заменить значение нужного фильтра, но это не работало 
                if(scheduleFieldFromFilter.GetName() == specFilterName) {

                    string filterOldValue = currentFilter.GetStringValue();

                    string format = GetStringFormatOrDefault(filterOldValue);

                    string newVal = String.Format(format, newFilterValue);

                    currentFilter.SetValue(newVal);

                    newScheduleFilters.Add(currentFilter);
                } else {
                    newScheduleFilters.Add(currentFilter);
                }
            }

            SpecificationDefinition.SetFilters(newScheduleFilters);
        }


        /// <summary>
        /// Метод размещает спецификацию на листе
        /// </summary>
        public ScheduleSheetInstance PlaceSpec(SheetHelper sheetHelper) {

            ScheduleSheetInstance newScheduleSheetInstance = ScheduleSheetInstance.Create(
                    Repository.Document,
                    sheetHelper.Sheet.Id,
                    Specification.Id,
                    SpecSheetInstancePoint);

            return newScheduleSheetInstance;
        }
    }
}
