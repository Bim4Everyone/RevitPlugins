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

        private readonly string _costsSpecMark = "обр_ФОП_Марка ведомости расхода";
        private readonly string _documentationSet = "обр_ФОП_Раздел проекта";
        private readonly string _section = "ФОП_Секция СМР";
        private readonly string _organizationalLevel = "обр_ФОП_Орг. уровень";

        // Если номер формы у арматуры == 10000, то это семейства-оболочки, которые управляют другой арматурой, их не считаем
        private readonly string _paramForRebarShell = "обр_ФОП_Форма_номер";
        private readonly int _paramValueForRebarShell = 10000;

        // Если у опалубки марка бетона == 0, то их не считаем
        private readonly string _paramForFormClass = "обр_ФОП_Марка бетона B";
        private readonly double _paramValueForFormClass = 0;

        private List<Element> _elementsForAnalize;
        private readonly List<string> _paramsForAll;


        public DesignTypeListAnalyzer(RevitRepository revitRepository, ParamUtils paramUtils, ElementFactory elementFactory) {
            _revitRepository = revitRepository;
            _paramUtils = paramUtils;
            _elementFactory = elementFactory;

            _paramsForAll = new List<string>() {
                _costsSpecMark,
                _documentationSet,
                _section,
                _organizationalLevel
            };
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

                    // Если значение параметра указывает, что это оболочка, то пропускаем, этот элемент не участвует в расчетах
                    if(_paramUtils.GetParamValueAnywhere<int>(element, _paramForRebarShell) == _paramValueForRebarShell) {
                        continue;
                    }
                } else {
                    // Если это опалубка, то необходимо проверить армируется ли она, если нет, то работать с ней не нужно
                    // В соотвтетствии с ТЗ показателем того, что элемент несущий и будет армироваться является
                    // значение марки бетона отличное от 0

                    // Проверяем наличие параметра марки бетона у опалубки
                    if(!_paramUtils.HasParamAnywhere(element, _paramForFormClass)) {
                        continue;
                    }

                    // Если значение параметра указывает, что элемент не армируется, то этот элемент не участвует в расчетах
                    if(_paramUtils.GetParamValueAnywhere<double>(element, _paramForFormClass) == _paramValueForFormClass) {
                        continue;
                    }
                }

                // Проверяем у всех элементов наличие параметров, необходимых для распределения по типам конструкций
                if(!_paramUtils.HasParamsAnywhere(element, _paramsForAll)) {
                    continue;
                }

                // Получение значений параметров, необходимых для распределения по типам конструкций
                string typeName = element.GetParamValue<string>(_costsSpecMark);
                typeName = typeName ?? "";
                // Сделали преобразование null в "" из-за того, что фильтрация в GUI иначе нормально не отрабатывает
                string docPackage = element.GetParamValue<string>(_documentationSet) ?? "";
                string elemSection = element.GetParamValue<string>(_section) ?? "";

                // Следующий код реализован представленным образом в связи с проблемой, когда параметр с именем 
                // _organizationalLevel может иметь разный тип данных
                var organizationalLevelAsStr = element.GetParam(_organizationalLevel).AsValueString();
                bool aboveZero = !organizationalLevelAsStr.Contains('-');

                // Ищем подходящий тип конструкции среди уже существующих в списке
                DesignTypeVM designType = designTypes.FirstOrDefault(
                    e => e.TypeName == typeName
                      && e.DocPackage == docPackage
                      && e.ElemSection == elemSection
                      && e.AboveZero == aboveZero);

                // Если null, то создаем новый, если нет, то дописываем элемент в список уже существующего
                ICommonElement specificElement = _elementFactory.CreateSpecificElement(element);
                if(designType is null) {
                    var newDesignType = new DesignTypeVM(typeName, docPackage, elemSection, aboveZero, specificElement);
                    designTypes.Add(newDesignType);
                } else {
                    designType.AddItem(specificElement);
                }
            }
            return designTypes;
        }
    }
}
