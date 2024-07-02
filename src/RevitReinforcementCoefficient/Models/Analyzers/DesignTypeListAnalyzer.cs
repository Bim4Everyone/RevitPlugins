using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitReinforcementCoefficient.Models.ElementModels;
using RevitReinforcementCoefficient.ViewModels;

namespace RevitReinforcementCoefficient.Models.Analyzers {
    internal class DesignTypeListAnalyzer {
        private readonly RevitRepository _revitRepository;
        private readonly ParamUtils _paramUtils;
        private readonly ElementFactory _elementFactory;

        // Если номер формы у арматуры == 10000, то это семейства-оболочки, которые управляют другой арматурой, их не считаем
        private readonly string _paramForRebarShell = "обр_ФОП_Форма_номер";
        private readonly int _paramValueForRebarShell = 10000;

        private List<Element> _elementsForAnalize;

        // TODO в дальнейшем поля выполнить через настройки и отдельный класс
        private readonly List<string> _paramsForAll = new List<string>() {
            "обр_ФОП_Марка ведомости расхода",
            "обр_ФОП_Раздел проекта",
            "обр_ФОП_Орг. уровень"
        };


        public DesignTypeListAnalyzer(RevitRepository revitRepository, ParamUtils paramUtils, ElementFactory elementFactory) {
            _revitRepository = revitRepository;
            _paramUtils = paramUtils;
            _elementFactory = elementFactory;
        }

        /// <summary>
        /// Получает элементы с вида заданных по ТЗ категорий
        /// </summary>
        public bool GetElementsForAnalize() {
            _elementsForAnalize = _revitRepository.ElementsByFilterInActiveView;
            if(_elementsForAnalize is null || _elementsForAnalize.Count == 0) {
                return false;
            } else {
                return true;
            }
        }

        /// <summary>
        /// Проверяет наличие нужных параметров и распределяет элементы по типам конструкции
        /// </summary>
        public List<DesignTypeVM> CheckNSortByDesignTypes() {
            var designTypes = new List<DesignTypeVM>();

            foreach(Element element in _elementsForAnalize) {
                // Проверяем, только если это арматура
                if(element.InAnyCategory(BuiltInCategory.OST_Rebar)) {
                    // Отсеиваем арматуры с номером формы == 10000 - это семейства-оболочки, которые управляют другой арматурой
                    // Проверяем у арматуры наличие параметра, по которому определяется семейство-оболочка
                    if(!_paramUtils.HasParamAnywhere(element, _paramForRebarShell)) {
                        continue;
                    }

                    // Если значение параметра указывает, что это облочка, то пропускаем, этот элемент не участвует в расчетах
                    if(_paramUtils.GetParamValueAnywhere<int>(element, _paramForRebarShell) == _paramValueForRebarShell) {
                        continue;
                    }
                }

                // Проверяем у всех элементов наличие параметров, необходимых для распределения по типам конструкций
                if(!_paramUtils.HasParamsAnywhere(element, _paramsForAll)) {
                    continue;
                }

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
                ICommonElement specificElement = _elementFactory.CreateSpecificElement(element);
                if(designType is null) {
                    var newDesignType = new DesignTypeVM(typeName, docPackage, aboveZero, specificElement);
                    designTypes.Add(newDesignType);
                } else {
                    designType.AddItem(specificElement);
                }
            }
            return designTypes;
        }
    }
}
