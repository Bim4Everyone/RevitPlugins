using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Revit;

using RevitReinforcementCoefficient.ViewModels;


namespace RevitReinforcementCoefficient.Models {
    internal class DesignTypeAnalyzer {
        private readonly ParamUtils _paramUtils;

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

        public DesignTypeAnalyzer(ParamUtils paramUtils) {

            _paramUtils = paramUtils;
        }

        /// <summary>
        /// Проверяет наличие нужных параметров и распределяет элементы по типам конструкции
        /// </summary>
        public List<DesignTypeInfoVM> CheckNSortByDesignTypes(IEnumerable<Element> allElements, ReportVM report) {
            List<DesignTypeInfoVM> designTypes = new List<DesignTypeInfoVM>();

            foreach(Element element in allElements) {
                // Проверяем, только если это арматура
                if(element.Category.GetBuiltInCategory() == BuiltInCategory.OST_Rebar) {
                    // Проверяем у арматуры наличие параметра, по которому определяется семейство-оболочка
                    if(!_paramUtils.HasParamAnywhere(element, _paramForRebarShell, report)) {
                        // TODO добавлять в отчет
                        continue;
                    }

                    // Если значение параметра указывает, что это облочка, то пропускаем, этот элемент не участвует в расчетах
                    if(_paramUtils.GetParamValueAnywhere<int>(element, _paramForRebarShell) == 1000) {
                        continue;
                    }
                }

                // Проверяем у всех элементов наличие параметров, необходимых для распределения по типам конструкций
                if(!_paramUtils.HasParamsAnywhere(element, _paramsForAll, report)) {
                    continue;
                }

                // Пока пусть так, дальше нужно сделать в зависимости от уровня
                // Получение значений параметров, необходимых для распределения по типам конструкций
                string typeName = element.GetParamValue<string>("обр_ФОП_Марка ведомости расхода");
                typeName = typeName is null ? "" : typeName;
                string docPackage = element.GetParamValue<string>("обр_ФОП_Раздел проекта");
                docPackage = docPackage is null ? "" : docPackage;
                bool aboveZero = element.GetParamValue<int>("обр_ФОП_Орг. уровень") > 0;
                // Сделали преобразование null в "" из-за того, что фильтрация в GUI иначе нормально не отрабатывает

                // Ищем подходящий тип конструкции среди уже существующих в списке
                DesignTypeInfoVM designType = designTypes.FirstOrDefault(
                    e => e.TypeName == typeName && e.DocPackage == docPackage && e.AboveZero == aboveZero);

                // Если null, то создаем новый, если нет, то дописываем элемент в список уже существующего
                if(designType is null) {
                    DesignTypeInfoVM newDesignType = new DesignTypeInfoVM(typeName, docPackage, aboveZero);

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
        public void CheckParamsInFormElements(DesignTypeInfoVM designType, ReportVM report) {
            foreach(Element elem in designType.Elements) {
                if(!_paramUtils.HasParamsAnywhere(elem, _paramsForFormElements, report)) {
                    designType.HasErrors = true;
                }
            }
            designType.FormParamsChecked = true;
        }


        /// <summary>
        /// Проверяет наличие параметров у арматуры по типу конструкции
        /// </summary>
        public void CheckParamsInRebars(DesignTypeInfoVM designType, ReportVM report) {
            foreach(Element rebar in designType.Rebars) {
                // Далее проверяем параметры, которые должны быть у всех элементов арматуры
                if(!_paramUtils.HasParamsAnywhere(rebar, _paramsForRebars, report)) {
                    designType.HasErrors = true;
                }

                // Здесь проверяем разные параметры, которые должны быть у системной/IFC арматуры
                // Если элемент класса Rebar (т.е. системная арматура)
                if(rebar is FamilyInstance) {
                    if(!_paramUtils.HasParamsAnywhere(rebar, _paramsForIfcRebars, report)) {
                        designType.HasErrors = true;
                    }
                } else {
                    if(!_paramUtils.HasParamsAnywhere(rebar, _paramsForSysRebars, report)) {
                        designType.HasErrors = true;
                    }
                }
            }
            designType.RebarParamsChecked = true;
        }
    }
}
