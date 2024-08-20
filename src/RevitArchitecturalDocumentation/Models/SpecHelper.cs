using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitArchitecturalDocumentation.Models {
    internal class SpecHelper {
        public SpecHelper(RevitRepository revitRepository, ScheduleSheetInstance scheduleSheetInstance, TreeReportNode report = null) {

            Report = report;
            Repository = revitRepository;

            SpecSheetInstance = scheduleSheetInstance;
            SpecSheetInstancePoint = scheduleSheetInstance.Point;
            Specification = scheduleSheetInstance.Document.GetElement(scheduleSheetInstance.ScheduleId) as ViewSchedule;
            SpecificationDefinition = Specification.Definition;
            SpecificationFilters = SpecificationDefinition.GetFilters().ToList();
            NameHelper = new ViewNameHelper(Specification);
        }

        public SpecHelper(RevitRepository revitRepository, ViewSchedule viewSchedule, TreeReportNode report = null) {

            Report = report;
            Repository = revitRepository;

            Specification = viewSchedule;
            SpecificationDefinition = Specification.Definition;
            SpecificationFilters = SpecificationDefinition.GetFilters().ToList();
            NameHelper = new ViewNameHelper(Specification);
        }

        public TreeReportNode Report { get; set; }
        public RevitRepository Repository { get; set; }
        public ScheduleSheetInstance SpecSheetInstance { get; set; }
        public XYZ SpecSheetInstancePoint { get; set; }
        public ViewSchedule Specification { get; set; }
        public ScheduleDefinition SpecificationDefinition { get; set; }
        public List<ScheduleFilter> SpecificationFilters { get; set; }
        public List<string> SpecFilterNames { get; set; }
        public ViewNameHelper NameHelper { get; set; }


        /// <summary>
        /// Метод получает список имен полей спецификации, которые применяются в фильтрах спеки
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
        /// Метод находит в проекте, а если не нашел, то создает спецификацию с указанным именем и задает ей фильтрацию
        /// </summary>
        public SpecHelper GetOrDublicateNSetSpec(string filterName, int numberOfLevelAsInt) {

            string specName = NameHelper.Prefix
                              + NameHelper.PrefixOfLevelBlock
                              + String.Format(NameHelper.LevelNumberFormat, numberOfLevelAsInt)
                              + NameHelper.SuffixOfLevelBlock
                              + NameHelper.Suffix;

            ViewSchedule newViewSpec = Repository.GetSpecByName(specName);
            SpecHelper newSpecHelper;
            // Если спеку с указанным именем не нашли, то будем создавать дублированием
            if(newViewSpec is null) {
                Report?.AddNodeWithName($"Спецификация с именем \"{specName}\" не найдена в проекте, приступаем к созданию");
                newViewSpec = Repository.Document.GetElement(Specification.Duplicate(ViewDuplicateOption.Duplicate)) as ViewSchedule;
                Report?.AddNodeWithName($"Спецификация успешно создана!");
                newViewSpec.Name = specName;
                Report?.AddNodeWithName($"Задано имя: {newViewSpec.Name}");

                newSpecHelper = new SpecHelper(Repository, newViewSpec, Report);
                newSpecHelper.ChangeSpecFilters(filterName, numberOfLevelAsInt);
            } else {
                Report?.AddNodeWithName($"Спецификация с именем \"{newViewSpec.Name}\" успешно найдена в проекте!");
                newSpecHelper = new SpecHelper(Repository, newViewSpec, Report);
            }
            newSpecHelper.SpecSheetInstancePoint = SpecSheetInstancePoint;

            return newSpecHelper;
        }


        /// <summary>
        /// Метод по изменению фильтра спецификации с указанным именем на указанное значение с учетом формата предыдущего значения
        /// </summary>
        public void ChangeSpecFilters(string specFilterName, int newFilterValue) {

            // В дальнейшем нужно предусмотреть проверки, что поле фильрации принимает строки + сеттеры для других типов
            List<ScheduleFilter> newScheduleFilters = new List<ScheduleFilter>();

            // Перебираем фильтры и записываем каждый, изменяя только тот, что ищем потому что механизм изменения значения конкретного фильтра работал нестабильно
            for(int i = 0; i < SpecificationFilters.Count; i++) {

                ScheduleFilter currentFilter = SpecificationFilters[i];

                ScheduleField scheduleFieldFromFilter = SpecificationDefinition.GetField(currentFilter.FieldId);

                if(scheduleFieldFromFilter.GetName() == specFilterName) {

                    string filterOldValue = currentFilter.GetStringValue();
                    string format = ViewNameHelper.GetStringFormatOrDefault(filterOldValue);
                    string newVal = String.Format(format, newFilterValue);
                    currentFilter.SetValue(newVal);

                    Report?.AddNodeWithName($"Фильтру задали значение {currentFilter.GetStringValue()}");
                    newScheduleFilters.Add(currentFilter);
                } else {
                    newScheduleFilters.Add(currentFilter);
                }
            }

            SpecificationDefinition.SetFilters(newScheduleFilters);
        }


        /// <summary>
        /// Размещает спецификацию на листе
        /// </summary>
        public ScheduleSheetInstance PlaceSpec(SheetHelper sheetHelper) {

            ScheduleSheetInstance newScheduleSheetInstance = ScheduleSheetInstance.Create(
                    Repository.Document,
                    sheetHelper.Sheet.Id,
                    Specification.Id,
                    SpecSheetInstancePoint);

            SpecSheetInstance = newScheduleSheetInstance;

            if(newScheduleSheetInstance is null) {
                Report?.AddNodeWithName($"❗ Не удалось создать видовой экран спецификации на листе!");
            } else {
                Report?.AddNodeWithName($"Видовой экран спецификации успешно создан на листе!");
            }

            return newScheduleSheetInstance;
        }
    }
}
