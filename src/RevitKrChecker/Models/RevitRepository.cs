using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.SimpleServices;

using RevitKrChecker.Models.Check;
using RevitKrChecker.Models.CheckOptions;
using RevitKrChecker.Models.CheckRule;
using RevitKrChecker.Models.Interfaces;

namespace RevitKrChecker.Models {
    internal class RevitRepository {
        private readonly ILocalizationService _localizationService;

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


        public RevitRepository(UIApplication uiApplication, ILocalizationService localizationService) {
            UIApplication = uiApplication;
            _localizationService = localizationService;
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
                    CheckRule = new ContainsCheckRule(_localizationService),
                    TrueValues = new List<string>() {
                        "Балка", "ЛМарш", "ЛПлощадка", "Парапет", "БПодготовка", "Подстилающий слой", "Колонна", "Пилон", "Свая", "Бортик",
                        "Термовкладыш", "Перекрытие", "Покрытие", "Засыпка", "Капитель", "Изоляция", "Стена", "Подпорная стена", "ФПлита" }
                },
                _localizationService));

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
                    CheckRule = new ContainsCheckRule(_localizationService),
                    TrueValues = new List<string>() { "Корпус", "Автостоянка", "Пристройка" }
                },
                _localizationService));

            // Параметр "Секция СМР" должен быть заполнен
            nonStoppingChecks.Add(new HasValueCheck(
                new HasValueCheckOptions() {
                    CheckName = "Проверка Секции",
                    TargetParamName = "ФОП_Секция СМР",
                    TargetParamLevel = ParamLevel.Instance
                },
                _localizationService));

            // Параметр "Этаж СМР" должен быть заполнен
            nonStoppingChecks.Add(new HasValueCheck(
                new HasValueCheckOptions() {
                    CheckName = "Проверка Этажа",
                    TargetParamName = "ФОП_Этаж СМР",
                    TargetParamLevel = ParamLevel.Instance
                },
                _localizationService));

            // Параметр "Материал: Имя" начинается с: г02.02, г02.03, г02.04
            nonStoppingChecks.Add(new ParamCheck(
                new ParamCheckOptions() {
                    CheckName = "Проверка имени материала",
                    TargetParamName = "Имя",
                    TargetParamLevel = ParamLevel.Material,
                    CheckRule = new StartWithCheckRule(_localizationService),
                    TrueValues = new List<string>() { "г02.02", "г02.03", "г02.04" }
                },
                _localizationService));

            // Параметр "Материал: Ключевая заметка" начинается с: г02.02, г02.03, г02.04
            nonStoppingChecks.Add(new ParamCheck(
                new ParamCheckOptions() {
                    CheckName = "Проверка ключевой заметки материала",
                    TargetParamName = "Ключевая пометка",
                    TargetParamLevel = ParamLevel.Material,
                    CheckRule = new StartWithCheckRule(_localizationService),
                    TrueValues = new List<string>() { "г02.02", "г02.03", "г02.04" }
                },
                _localizationService));

            // Параметр "Материал: Описание" заполнен
            nonStoppingChecks.Add(new HasValueCheck(
                new HasValueCheckOptions() {
                    CheckName = "Проверка описания материала",
                    TargetParamName = "Описание",
                    TargetParamLevel = ParamLevel.Material
                },
                _localizationService));

            // Параметр "ФОП_МТР_Наименование главы" равно: "Устройство фундамента", "Конструкции до отм. +/-0,000"
            // или "Конструкции выше отм. +/-0,000"
            nonStoppingChecks.Add(new ParamCheck(
                new ParamCheckOptions() {
                    CheckName = "Проверка главы материала",
                    TargetParamName = "ФОП_МТР_Наименование главы",
                    TargetParamLevel = ParamLevel.Material,
                    CheckRule = new EqualCheckRule(_localizationService),
                    TrueValues = new List<string>() {
                        "Устройство фундамента",
                        "Конструкции до отм. +/-0,000",
                        "Конструкции выше отм. +/-0,000"
                    }
                },
                _localizationService));

            // Параметр "ФОП_МТР_Наименование работы" должен быть заполнен
            nonStoppingChecks.Add(new HasValueCheck(
                new HasValueCheckOptions() {
                    CheckName = "Проверка наименования работы материала",
                    TargetParamName = "ФОП_МТР_Наименование работы",
                    TargetParamLevel = ParamLevel.Material
                },
                _localizationService));

            // Параметр "ФОП_МТР_Единица измерения" должен быть равен: "м", "м²", "м³", "шт."
            nonStoppingChecks.Add(new ParamCheck(
                new ParamCheckOptions() {
                    CheckName = "Проверка единицы измерения материала",
                    TargetParamName = "ФОП_МТР_Единица измерения",
                    TargetParamLevel = ParamLevel.Material,
                    CheckRule = new EqualCheckRule(_localizationService),
                    TrueValues = new List<string>() {
                                    "м",
                                    "м²",
                                    "м³",
                                    "шт."
                    }
                },
                _localizationService));

            // Параметр "ФОП_МТР_Тип подсчета" должен быть равен "1", "2", "3", "4"
            nonStoppingChecks.Add(new ParamCheck(
                new ParamCheckOptions() {
                    CheckName = "Проверка типа подсчета материала",
                    TargetParamName = "ФОП_МТР_Тип подсчета",
                    TargetParamLevel = ParamLevel.Material,
                    CheckRule = new EqualCheckRule(_localizationService),
                    TrueValues = new List<string>() {
                                                "1",
                                                "2",
                                                "3",
                                                "4"
                    }
                },
                _localizationService));

            // Значение параметра "Материал: Описание" содержится в "Материал: Имя"
            // Параметр "Материал: Имя" содержит параметр "Материал: Описание"
            nonStoppingChecks.Add(new CompareMaterialParamsCheck(
                new CompareCheckOptions() {
                    CheckName = "Проверка наличия описания материала в имени материала",
                    TargetParamName = "Имя",
                    TargetParamLevel = ParamLevel.Material,
                    CheckRule = new ContainsCheckRule(_localizationService),
                    SourceParamName = "Описание",
                    SourceParamLevel = ParamLevel.Material,
                },
                _localizationService));

            // Код работы в "Материал: Ключевая пометка" соответствует коду работы в "Материал: Имя" (содержится в нем)
            // Параметр "Материал: Имя" содержит параметр "Материал: Ключевая пометка"
            nonStoppingChecks.Add(new CompareMaterialParamsCheck(
                new CompareCheckOptions() {
                    CheckName = "Проверка наличия ключевой пометки материала в имени материала",
                    TargetParamName = "Имя",
                    TargetParamLevel = ParamLevel.Material,
                    CheckRule = new ContainsCheckRule(_localizationService),
                    SourceParamName = "Ключевая пометка",
                    SourceParamLevel = ParamLevel.Material,
                },
                _localizationService));

            // [ФОП_МТР_Единица измерения] соответствует [ФОП_МТР_Тип подсчета]:
            // ед.изм. "м" - подсчет "1";
            // ед.изм. "м²" - подсчет "2";
            // ед.изм. "м³" - подсчет "3";
            // ед.изм. "шт." - подсчет "4"
            // Параметр "ФОП_МТР_Единица измерения" содержит соответствующее "ФОП_МТР_Тип подсчета"
            nonStoppingChecks.Add(new TemplatesCompareMaterialParamsCheck(
                new TemplatesCompareCheckOptions() {
                    CheckName = "Проверка соответствия единицы измерения и типа подсчета материала",
                    TargetParamName = "ФОП_МТР_Единица измерения",
                    TargetParamLevel = ParamLevel.Material,
                    CheckRule = new EqualCheckRule(_localizationService),
                    SourceParamName = "ФОП_МТР_Тип подсчета",
                    SourceParamLevel = ParamLevel.Material,
                    DictForCompare = new Dictionary<string, string>() {
                        { "1", "м" },
                        { "2", "м²" },
                        { "3", "м³" },
                        { "4", "шт." }
                    },
                    DictForCompareRule = new EqualCheckRule(_localizationService)
                },
                _localizationService));

            // Код работы в "Материал: Ключевая пометка" соответствует "Имени типа" элемента:
            // код СОДЕРЖИТ "г02.02." - имя начинается "Ф_";
            // код СОДЕРЖИТ "г02.03." - имя начинается "НН_";
            // код СОДЕРЖИТ "г02.04." - имя начинается "ВН_"
            // Проверяем Имя типа, что оно соответствует по правилу Ключевой пометке, которая содержится в ключах словаря
            nonStoppingChecks.Add(new TemplatesCompareElemParamsCheck(
                new TemplatesCompareCheckOptions() {
                    CheckName = "Проверка соответствия имени типа и ключевой пометки материала",
                    TargetParamName = "Имя типа",
                    TargetParamLevel = ParamLevel.Type,
                    CheckRule = new StartWithCheckRule(_localizationService),
                    SourceParamName = "Ключевая пометка",
                    SourceParamLevel = ParamLevel.Material,
                    DictForCompare = new Dictionary<string, string>() {
                        { "г02.02.", "Ф_" },
                        { "г02.03.", "НН_" },
                        { "г02.04.", "ВН_" }
                    },
                    DictForCompareRule = new ContainsCheckRule(_localizationService)
                },
                _localizationService));

            // Код работы в [Материал: Ключевая пометка] соответствует [Материал: ФОП_МТР_Наименование главы]:
            // код СОДЕРЖИТ  "г02.02." - глава равно "Устройство фундамента";
            // код СОДЕРЖИТ  "г02.03." - глава равно "Конструкции до отм. +/-0,000";
            // код СОДЕРЖИТ  "г02.04." - глава равно "Конструкции выше отм. +/-0,000"
            // Проверяем ФОП_МТР_Наименование главы, что значение соответствует по правилу Ключевой пометке,
            // которая содержится в ключах словаря
            nonStoppingChecks.Add(new TemplatesCompareMaterialParamsCheck(
                new TemplatesCompareCheckOptions() {
                    CheckName = "Проверка соответствия наименование главы и ключевой пометки материала",
                    TargetParamName = "ФОП_МТР_Наименование главы",
                    TargetParamLevel = ParamLevel.Material,
                    CheckRule = new EqualCheckRule(_localizationService),
                    SourceParamName = "Ключевая пометка",
                    SourceParamLevel = ParamLevel.Material,
                    DictForCompare = new Dictionary<string, string>() {
                        { "г02.02.", "Устройство фундамента" },
                        { "г02.03.", "Конструкции до отм. +/-0,000" },
                        { "г02.04.", "Конструкции выше отм. +/-0,000" }
                    },
                    DictForCompareRule = new ContainsCheckRule(_localizationService)
                },
                _localizationService));

            return nonStoppingChecks;
        }
    }
}
