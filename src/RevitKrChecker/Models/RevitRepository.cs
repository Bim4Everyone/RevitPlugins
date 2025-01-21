using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using RevitKrChecker.Models.Check;
using RevitKrChecker.Models.CheckOptions;
using RevitKrChecker.Models.Interfaces;
using RevitKrChecker.Models.Rule;

namespace RevitKrChecker.Models {
    internal class RevitRepository {

        private readonly List<BuiltInCategory> _categoriesForWork = new List<BuiltInCategory>() {
            BuiltInCategory.OST_Walls,
            BuiltInCategory.OST_Floors,
            BuiltInCategory.OST_StructuralFoundation,
            BuiltInCategory.OST_GenericModel,
            BuiltInCategory.OST_StructuralFraming,
            BuiltInCategory.OST_StructuralColumns,
            BuiltInCategory.OST_Stairs,
            BuiltInCategory.OST_StructConnections,
            BuiltInCategory.OST_Roofs
        };


        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;
        }

        public UIApplication UIApplication { get; }
        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;
        public Application Application => UIApplication.Application;
        public Document Document => ActiveUIDocument.Document;

        public List<Element> GetViewElements() {
            var filter = new ElementMulticategoryFilter(_categoriesForWork);
            return new FilteredElementCollector(Document, Document.ActiveView.Id)
                .WherePasses(filter)
                .WhereElementIsNotElementType()
                .ToElements()
                .ToList();
        }

        public List<Element> GetPjElements() {
            var filter = new ElementMulticategoryFilter(_categoriesForWork);
            return new FilteredElementCollector(Document)
                .WherePasses(filter)
                .WhereElementIsNotElementType()
                .ToElements()
                .ToList();
        }

        public List<ICheck> StoppingChecks() {
            List<ICheck> stoppingChecks = new List<ICheck>();

            // Проверка, что в имени типа есть индекс типа конструкции
            stoppingChecks.Add(new ParamCheck(
                new ParamCheckOptions() {
                    CheckName = "Проверка наличия типа конструкции в имени типоразмера",
                    TargetParamName = "Имя типа",
                    TargetParamLevel = ParamLevel.Type,
                    CheckRule = new ContainsCheckRule(),
                    TrueValues = new List<string>() {
                        "Балка", "ЛМарш", "ЛПлощадка", "Парапет", "БПодготовка", "Подстилающий слой", "Колонна", "Пилон", "Свая", "Бортик",
                        "Термовкладыш", "Перекрытие", "Покрытие", "Засыпка", "Капитель", "Изоляция", "Стена", "Подпорная стена", "ФПлита" }
                }));

            return stoppingChecks;
        }

        public List<ICheck> NonStoppingChecks() {
            List<ICheck> nonStoppingChecks = new List<ICheck>();

            // Параметр "ФОП_Блок СМР" должен содержать "Корпус", "Автостоянка", "Пристройка"
            nonStoppingChecks.Add(new ParamCheck(
                new ParamCheckOptions() {
                    CheckName = "Проверка Блока",
                    TargetParamName = "ФОП_Блок СМР",
                    TargetParamLevel = ParamLevel.Instance,
                    CheckRule = new ContainsCheckRule(),
                    TrueValues = new List<string>() { "Корпус", "Автостоянка", "Пристройка" }
                }));

            // Параметр "Секция СМР" должен быть заполнен
            nonStoppingChecks.Add(new HasValueCheck(
                new HasValueCheckOptions() {
                    CheckName = "Проверка Секции",
                    TargetParamName = "ФОП_Секция СМР",
                    TargetParamLevel = ParamLevel.Instance
                }));

            // Параметр "Этаж СМР" должен быть заполнен
            nonStoppingChecks.Add(new HasValueCheck(
                new HasValueCheckOptions() {
                    CheckName = "Проверка Этажа",
                    TargetParamName = "ФОП_Этаж СМР",
                    TargetParamLevel = ParamLevel.Instance
                }));

            // Параметр "Материал: Имя" начинается с: г02.02, г02.03, г02.04
            nonStoppingChecks.Add(new ParamCheck(
                new ParamCheckOptions() {
                    CheckName = "Проверка имени материала",
                    TargetParamName = "Имя",
                    TargetParamLevel = ParamLevel.Material,
                    CheckRule = new StartWithCheckRule(),
                    TrueValues = new List<string>() { "г02.02", "г02.03", "г02.04" }
                }));

            // Параметр "Материал: Ключевая заметка" начинается с: г02.02, г02.03, г02.04
            nonStoppingChecks.Add(new ParamCheck(
                new ParamCheckOptions() {
                    CheckName = "Проверка ключевой заметки материала",
                    TargetParamName = "Ключевая пометка",
                    TargetParamLevel = ParamLevel.Material,
                    CheckRule = new StartWithCheckRule(),
                    TrueValues = new List<string>() { "г02.02", "г02.03", "г02.04" }
                }));

            // Параметр "Материал: Описание" заполнен
            nonStoppingChecks.Add(new HasValueCheck(
                new HasValueCheckOptions() {
                    CheckName = "Проверка описания материала",
                    TargetParamName = "Описание",
                    TargetParamLevel = ParamLevel.Material
                }));

            // Параметр "ФОП_МТР_Наименование главы" равно: "Устройство фундамента", "Конструкции до отм. +/-0,000"
            // или "Конструкции выше отм. +/-0,000"
            nonStoppingChecks.Add(new ParamCheck(
                new ParamCheckOptions() {
                    CheckName = "Проверка главы материала",
                    TargetParamName = "ФОП_МТР_Наименование главы",
                    TargetParamLevel = ParamLevel.Material,
                    CheckRule = new EqualCheckRule(),
                    TrueValues = new List<string>() {
                        "Устройство фундамента",
                        "Конструкции до отм. +/-0,000",
                        "Конструкции выше отм. +/-0,000"
                    }
                }));

            // Параметр "ФОП_МТР_Наименование работы" должен быть заполнен
            nonStoppingChecks.Add(new HasValueCheck(
                new HasValueCheckOptions() {
                    CheckName = "Проверка наименования работы материала",
                    TargetParamName = "ФОП_МТР_Наименование работы",
                    TargetParamLevel = ParamLevel.Material
                }));

            // Параметр "ФОП_МТР_Единица измерения" должен быть равен: "м", "м²", "м³", "шт."
            nonStoppingChecks.Add(new ParamCheck(
                new ParamCheckOptions() {
                    CheckName = "Проверка единицы измерения материала",
                    TargetParamName = "ФОП_МТР_Единица измерения",
                    TargetParamLevel = ParamLevel.Material,
                    CheckRule = new EqualCheckRule(),
                    TrueValues = new List<string>() {
                                    "м",
                                    "м²",
                                    "м³",
                                    "шт."
                    }
                }));

            // Параметр "ФОП_МТР_Тип подсчета" должен быть равен "1", "2", "3", "4"
            nonStoppingChecks.Add(new ParamCheck(
                new ParamCheckOptions() {
                    CheckName = "Проверка типа подсчета материала",
                    TargetParamName = "ФОП_МТР_Тип подсчета",
                    TargetParamLevel = ParamLevel.Material,
                    CheckRule = new EqualCheckRule(),
                    TrueValues = new List<string>() {
                                                "1",
                                                "2",
                                                "3",
                                                "4"
                    }
                }));

            // Значение параметра "Материал: Описание" содержится в "Материал: Имя"
            // Параметр "Материал: Имя" содержит параметр "Материал: Описание"
            nonStoppingChecks.Add(new CompareMaterialParamsCheck(
                new CompareCheckOptions() {
                    CheckName = "Проверка описания материала в имени материала",
                    TargetParamName = "Имя",
                    TargetParamLevel = ParamLevel.Material,
                    CheckRule = new ContainsCheckRule(),
                    SourceParamName = "Описание",
                    SourceParamLevel = ParamLevel.Material,
                }));


            return nonStoppingChecks;
        }
    }
}
