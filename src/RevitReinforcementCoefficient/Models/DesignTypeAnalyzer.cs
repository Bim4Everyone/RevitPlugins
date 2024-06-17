using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Revit;

using RevitReinforcementCoefficient.Models.ElementModels;
using RevitReinforcementCoefficient.ViewModels;


namespace RevitReinforcementCoefficient.Models {
    internal class DesignTypeAnalyzer {

        // TODO в дальнейшем поля выполнить через настройки и отдельный класс
        private readonly List<string> _paramsForAll = new List<string>() {
            "обр_ФОП_Марка ведомости расхода",
            "обр_ФОП_Раздел проекта",
            "обр_ФОП_Орг. уровень"
        };

        private readonly List<string> _paramsForFormElements = new List<string>() { "ФОП_ТИП_Армирование" };

        private readonly string _paramForRebarShell = "обр_ФОП_Форма_номер";

        private readonly List<string> _paramsForRebars = new List<string>() {
            "мод_ФОП_Диаметр",
            "обр_ФОП_Длина",
            "обр_ФОП_Расчет в погонных метрах",
            "обр_ФОП_Количество типовых на этаже",
            "обр_ФОП_Количество типовых этажей"
        };

        private readonly List<string> _paramsForSysRebars = new List<string>() {
            "Полная длина стержня",
            "Количество"
        };

        private readonly List<string> _paramsForIfcRebars = new List<string>() { "обр_ФОП_Количество" };

        private readonly List<BuiltInCategory> _rebarBIC = new List<BuiltInCategory>() { BuiltInCategory.OST_Rebar };

        public DesignTypeAnalyzer() { }

        /// <summary>
        /// Проверяет наличие нужных параметров и распределяет элементы по типам конструкции
        /// </summary>
        public List<DesignTypeVM> CheckNSortByDesignTypes(IEnumerable<Element> allElements, ReportVM report) {
            List<DesignTypeVM> designTypes = new List<DesignTypeVM>();

            foreach(Element element in allElements) {
                // Проверяем, только если это арматура
                if(element.InAnyCategory(_rebarBIC)) {
                    // Отсеиваем арматуры с номером формы == 1000 - это семейства-оболочки, которые управляют другой арматурой
                    // Проверяем у арматуры наличие параметра, по которому определяется семейство-оболочка
                    if(!ParamUtils.HasParamAnywhere(element, _paramForRebarShell, report)) {
                        continue;
                    }

                    // Если значение параметра указывает, что это облочка, то пропускаем, этот элемент не участвует в расчетах
                    if(ParamUtils.GetParamValueAnywhere<int>(element, _paramForRebarShell) == 1000) {
                        continue;
                    }
                }

                // Проверяем у всех элементов наличие параметров, необходимых для распределения по типам конструкций
                if(!ParamUtils.HasParamsAnywhere(element, _paramsForAll, report)) {
                    continue;
                }

                // Пока пусть так, дальше нужно сделать в зависимости от уровня
                // Получение значений параметров, необходимых для распределения по типам конструкций
                string typeName = element.GetParamValue<string>("обр_ФОП_Марка ведомости расхода");
                typeName = typeName ?? "";
                string docPackage = element.GetParamValue<string>("обр_ФОП_Раздел проекта");
                docPackage = docPackage ?? "";
                // Сделали преобразование null в "" из-за того, что фильтрация в GUI иначе нормально не отрабатывает
                bool aboveZero = element.GetParamValue<int>("обр_ФОП_Орг. уровень") > 0;

                // Ищем подходящий тип конструкции среди уже существующих в списке
                DesignTypeVM designType = designTypes.FirstOrDefault(
                    e => e.TypeName == typeName && e.DocPackage == docPackage && e.AboveZero == aboveZero);

                // Если null, то создаем новый, если нет, то дописываем элемент в список уже существующего
                if(designType is null) {
                    DesignTypeVM newDesignType = new DesignTypeVM(typeName, docPackage, aboveZero);

                    newDesignType.AddItem(element);
                    designTypes.Add(newDesignType);
                } else {
                    designType.AddItem(element);
                }
            }
            return designTypes;
        }


        /// <summary>
        /// Проверяет наличие параметров у опалубки по типу конструкции
        /// </summary>
        public void CheckParamsInFormElements(DesignTypeVM designType, ReportVM report) {
            foreach(ICommonElement elem in designType.Formworks) {
                if(!ParamUtils.HasParamsAnywhere(elem.RevitElement, _paramsForFormElements, report)) {
                    designType.HasErrors = true;
                }
            }
            designType.FormParamsChecked = true;
        }


        /// <summary>
        /// Проверяет наличие параметров у арматуры по типу конструкции
        /// </summary>
        public void CheckParamsInRebars(DesignTypeVM designType, ReportVM report) {
            foreach(ICommonElement rebar in designType.Rebars) {
                // Далее проверяем параметры, которые должны быть у всех элементов арматуры
                if(!ParamUtils.HasParamsAnywhere(rebar.RevitElement, _paramsForRebars, report)) {
                    designType.HasErrors = true;
                }

                // Здесь проверяем разные параметры, которые должны быть у системной/IFC арматуры
                // Если элемент класса Rebar (т.е. системная арматура)
                if(rebar is FamilyInstance) {
                    if(!ParamUtils.HasParamsAnywhere(rebar.RevitElement, _paramsForIfcRebars, report)) {
                        designType.HasErrors = true;
                    }
                } else {
                    if(!ParamUtils.HasParamsAnywhere(rebar.RevitElement, _paramsForSysRebars, report)) {
                        designType.HasErrors = true;
                    }
                }
            }
            designType.RebarParamsChecked = true;
        }
    }
}
