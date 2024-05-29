using System;
using System.Linq;

using Autodesk.Revit.DB;

using Document = Autodesk.Revit.DB.Document;

namespace RevitArchitecturalDocumentation.Models {
    internal class SheetHelper {
        public SheetHelper(RevitRepository revitRepository, TreeReportNode report = null) {
            Repository = revitRepository;
            Report = report;
            NameHelper = new ViewNameHelper(null);
        }
        public SheetHelper(RevitRepository revitRepository, ViewSheet sheet, TreeReportNode report = null) {
            Repository = revitRepository;
            Sheet = sheet;
            Report = report;
            NameHelper = new ViewNameHelper(sheet);
        }


        public RevitRepository Repository { get; set; }
        public ViewSheet Sheet { get; set; }
        public ViewNameHelper NameHelper { get; set; }
        public TreeReportNode Report { get; set; }


        /// <summary>
        /// Находит в проекте, а если не нашел, то создает лист с указанным именем.
        /// </summary>
        public ViewSheet GetOrCreateSheet(string newSheetName, FamilySymbol titleBlockType, string widthParamName = "", string heightParamName = "", int width = 0, int height = 0) {

            ViewSheet newSheet = Repository.GetSheetByName(newSheetName);
            Sheet = newSheet;
            if(newSheet is null) {
                Report?.AddNodeWithName($"Лист с именем \"{newSheetName}\" не найден в проекте, приступаем к созданию");
                try {
                    CreateSheet(newSheetName, titleBlockType);

                    if(widthParamName.Length != 0 && heightParamName.Length != 0) {
                        SetUpSheetDimensions(widthParamName, heightParamName, width, height);
                    }
                } catch(Exception) {
                    Report?.AddNodeWithName($"❗ Произошла ошибка при создании листа!");
                }
            } else {
                Report?.AddNodeWithName($"Лист с именем \"{newSheetName}\" успешно найден в проекте!");

                NameHelper = new ViewNameHelper(newSheet);
                NameHelper.AnalyzeNGetLevelNumber();
            }

            return newSheet;
        }



        /// <summary>
        /// Создает лист, задает ему имя и тип рамки
        /// </summary>
        public ViewSheet CreateSheet(string newSheetName, FamilySymbol titleBlockType) {

            if(newSheetName.Length == 0) {
                Report?.AddNodeWithName($"❗ Произошла ошибка при работе с листом! Передано некорректное имя для задания!");

                return null;
            }
            if(titleBlockType is null) {
                Report?.AddNodeWithName($"❗ Произошла ошибка при работе с листом! Передана некорректный тип рамки листа!");
                return null;
            }

            ViewSheet newSheet = null;
            try {
                newSheet = ViewSheet.Create(Repository.Document, titleBlockType.Id);
                Report?.AddNodeWithName($"Лист успешно создан!");
                newSheet.Name = newSheetName;
                Report?.AddNodeWithName($"Задано имя: {newSheet.Name}");
            } catch(Exception) {
                Report?.AddNodeWithName($"❗ Произошла ошибка при создании листа!");
            }

            Sheet = newSheet;
            NameHelper = new ViewNameHelper(Sheet);
            NameHelper.AnalyzeNGetLevelNumber();
            return newSheet;
        }


        /// <summary>
        /// Задает габариты рамки листа через указанные параметры
        /// </summary>
        public void SetUpSheetDimensions(string widthParamName, string heightParamName, int width, int height) {

            if(widthParamName.Length == 0) {
                Report?.AddNodeWithName($"❗ Произошла ошибка при работе с листом! Передано некорректное имя параметра ширины рамки листа!");
                return;
            }
            if(heightParamName.Length == 0) {
                Report?.AddNodeWithName($"❗ Произошла ошибка при работе с листом! Передано некорректное имя параметра высоты рамки листа!");
                return;
            }
            if(width == 0) {
                Report?.AddNodeWithName($"❗ Произошла ошибка при работе с листом! Передано некорректное значение для задания ширины рамки листа!");
                return;
            }
            if(height == 0) {
                Report?.AddNodeWithName($"❗ Произошла ошибка при работе с листом! Передано некорректное значение для задания высоты рамки листа!");
                return;
            }

            try {
                // Ищем рамку на листе
                FamilyInstance titleBlock = new FilteredElementCollector(Repository.Document, Sheet.Id)
                    .OfCategory(BuiltInCategory.OST_TitleBlocks)
                    .WhereElementIsNotElementType()
                    .FirstOrDefault() as FamilyInstance;

                if(titleBlock is null) {
                    Report?.AddNodeWithName($"❗ Произошла ошибка при работе с листом! Не найдена рамка листа!");
                    return;

                } else {
                    Parameter widthParam = titleBlock.LookupParameter(widthParamName);
                    Parameter heightParam = titleBlock.LookupParameter(heightParamName);

                    if(widthParam != null && heightParam != null) {
                        widthParam.Set(UnitUtilsHelper.ConvertToInternalValue(width));
                        heightParam.Set(UnitUtilsHelper.ConvertToInternalValue(height));
                        Report?.AddNodeWithName($"Заданы габариты рамки: {width}х{height}");
                    } else {
                        Report?.AddNodeWithName($"❗ Произошла ошибка при работе с листом! У рамки не найден один из габаритных параметров!");
                        return;
                    }
                    Repository.Document.Regenerate();
                }

            } catch(Exception) {
                Report?.AddNodeWithName($"❗ Произошла ошибка при создании листа!");
            }
        }


        /// <summary>
        /// Проверяет есть ли спецификация с указанным именем на листе
        /// </summary>
        public bool HasSpecWithName(string specName) {

            Document doc = Sheet.Document;
            ScheduleSheetInstance schedule = Sheet.GetDependentElements(new ElementClassFilter(typeof(ScheduleSheetInstance)))
                .Select(id => doc.GetElement(id) as ScheduleSheetInstance)
                .FirstOrDefault(v => v.Name == specName);

            if(schedule is null) {
                return false;
            } else {
                return true;
            }
        }
    }
}
