using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

using Autodesk.Revit.DB;

using RevitArchitecturalDocumentation.ViewModels;

namespace RevitArchitecturalDocumentation.Models {
    internal class SheetHelper {
        public SheetHelper(RevitRepository revitRepository, StringBuilder report = null) {
            Repository = revitRepository;
            Report = report;
        }
        public SheetHelper(RevitRepository revitRepository, ViewSheet sheet, StringBuilder report = null) {
            Repository = revitRepository;
            Sheet = sheet;
            Report = report;
        }


        public StringBuilder Report { get; set; }
        public RevitRepository Repository { get; set; }
        public ViewSheet Sheet { get; set; }

        public int NumberOfLevel { get; set; }
        public bool HasProblemWithLevelName { get; set; } = true;

        

        /// <summary>
        /// Находит в проекте, а если не нашел, то создает лист с указанным именем.
        /// </summary>
        public ViewSheet GetOrCreateSheet(string newSheetName, FamilySymbol titleBlockType, string widthParamName = "", string heightParamName = "", int width = 0, int height = 0) {

            ViewSheet newSheet = Repository.GetSheetByName(newSheetName);
            Sheet = newSheet;
            if(newSheet is null) {               
                Report?.AppendLine($"                Лист с именем {newSheetName} не найден в проекте, приступаем к созданию");
                try {
                    CreateSheet(newSheetName, titleBlockType);

                    if(widthParamName.Length != 0 && heightParamName.Length != 0) {
                        SetUpSheetDimensions(widthParamName, heightParamName, width, height);
                    }
                    //Repository.Document.Regenerate();
                } catch(Exception) {
                    Report?.AppendLine($"❗               Произошла ошибка при создании листа!");
                }
            } else {
                Report?.AppendLine($"                Лист с именем {newSheetName} успешно найден в проекте!");
            }

            return newSheet;
        }



        /// <summary>
        /// Создает лист, задает ему имя и тип рамки
        /// </summary>
        public ViewSheet CreateSheet(string newSheetName, FamilySymbol titleBlockType) {

            if(newSheetName.Length == 0) {
                Report?.AppendLine($"❗               Произошла ошибка при работе с листом! Передано некорректное имя для задания!");
                return null;
            }
            if(titleBlockType is null) {
                Report?.AppendLine($"❗               Произошла ошибка при работе с листом! Передана некорректный тип рамки листа!");
                return null;
            }

            ViewSheet newSheet = null;
            try {
                newSheet = ViewSheet.Create(Repository.Document, titleBlockType.Id);
                Report?.AppendLine($"                Лист успешно создан!");
                newSheet.Name = newSheetName;
                Report?.AppendLine($"                Задано имя: {newSheet.Name}");

            } catch(Exception) {
                Report?.AppendLine($"❗               Произошла ошибка при создании листа!");
            }

            Sheet = newSheet;
            GetNumberOfLevel();
            return newSheet;
        }


        /// <summary>
        /// Задает габариты рамки листа через указанные параметры
        /// </summary>
        public void SetUpSheetDimensions(string widthParamName, string heightParamName, int width, int height) {

            if(widthParamName.Length == 0) {
                Report?.AppendLine($"❗               Произошла ошибка при работе с листом! Передано некорректное имя параметра ширины рамки листа!");
                return;
            }
            if(heightParamName.Length == 0) {
                Report?.AppendLine($"❗               Произошла ошибка при работе с листом! Передано некорректное имя параметра высоты рамки листа!");
                return;
            }
            if(width == 0) {
                Report?.AppendLine($"❗               Произошла ошибка при работе с листом! Передано некорректное значение для задания ширины рамки листа!");
                return;
            }
            if(height == 0) {
                Report?.AppendLine($"❗               Произошла ошибка при работе с листом! Передано некорректное значение для задания высоты рамки листа!");
                return;
            }

            try {
                // Ищем рамку на листе
                FamilyInstance titleBlock = new FilteredElementCollector(Repository.Document, Sheet.Id)
                    .OfCategory(BuiltInCategory.OST_TitleBlocks)
                    .WhereElementIsNotElementType()
                    .FirstOrDefault() as FamilyInstance;

                if(titleBlock is null) {
                    Report?.AppendLine($"❗               Произошла ошибка при работе с листом! Не найдена рамка листа!");
                    return;

                } else {
                    Parameter widthParam = titleBlock.LookupParameter(widthParamName);
                    Parameter heightParam = titleBlock.LookupParameter(heightParamName);

                    if(widthParam != null && heightParam != null) {
                        widthParam.Set(UnitUtilsHelper.ConvertToInternalValue(width));
                        heightParam.Set(UnitUtilsHelper.ConvertToInternalValue(height));
                        Report?.AppendLine($"                Заданы габариты рамки: {width}х{height}");
                    } else {
                        Report?.AppendLine($"❗               Произошла ошибка при работе с листом! У рамки не найден один из параметров!");
                        return;
                    }
                    Repository.Document.Regenerate();
                }

            } catch(Exception) {
                Report?.AppendLine($"❗               Произошла ошибка при создании листа!");
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


        /// <summary>
        /// Получает номер этажа по имени листа. Указывает через HasProblemWithLevelName, получилось ли это сделать
        /// </summary>
        public void GetNumberOfLevel() {

            if(Sheet is null) {
                return;
            }

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
