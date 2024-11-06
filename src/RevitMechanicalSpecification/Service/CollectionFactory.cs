using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using dosymep.Revit;
using System.Windows.Forms;
using RevitMechanicalSpecification.Entities;
using RevitMechanicalSpecification.Models;
using Autodesk.Revit.UI;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Bim4Everyone;


namespace RevitMechanicalSpecification.Service {
    internal class CollectionFactory {
        private readonly Document _document;
        private readonly SpecConfiguration _specConfiguration;
        private readonly UIDocument _uidocument;
        private readonly List<BuiltInCategory> _mechanicalCategories;
        private readonly bool _isNumberCurrencyExists;
        private readonly List<BuiltInCategory> _elementsCanBeInsulated;

        public CollectionFactory(Document doc, SpecConfiguration specConfiguration, UIDocument uIDocument) {
            _document = doc;
            // временное дополнение. Если в проекте есть ФОП_ВИС_Число ДЕ - группы должны обрабатываться.
            _isNumberCurrencyExists = _document.IsExistsParam(SharedParamsConfig.Instance.VISSpecNumbersCurrency);
            _uidocument = uIDocument;
            _specConfiguration = specConfiguration;
            _mechanicalCategories = new List<BuiltInCategory>()
            {
                BuiltInCategory.OST_DuctFitting,
                BuiltInCategory.OST_PipeFitting,
                BuiltInCategory.OST_PipeCurves,
                BuiltInCategory.OST_DuctCurves,
                BuiltInCategory.OST_FlexDuctCurves,
                BuiltInCategory.OST_FlexPipeCurves,
                BuiltInCategory.OST_DuctTerminal,
                BuiltInCategory.OST_DuctAccessory,
                BuiltInCategory.OST_PipeAccessory,
                BuiltInCategory.OST_MechanicalEquipment,
                BuiltInCategory.OST_DuctInsulations,
                BuiltInCategory.OST_PipeInsulations,
                BuiltInCategory.OST_PlumbingFixtures,
                BuiltInCategory.OST_Sprinklers,
                BuiltInCategory.OST_CableTray
            };
            _elementsCanBeInsulated = new List<BuiltInCategory>() {
                BuiltInCategory.OST_DuctFitting,
                BuiltInCategory.OST_PipeFitting,
                BuiltInCategory.OST_PipeCurves,
                BuiltInCategory.OST_DuctCurves
            };
        }

        /// <summary>
        /// Получение списка систем труб и воздуховодов и создание из них списка VisSystem-ов
        /// </summary>
        /// <returns></returns>
        public List<VisSystem> GetVisSystems() {
            List<Element> elements = GetElementsByCategories(
                new List<BuiltInCategory>() {
                BuiltInCategory.OST_PipingSystem,
                BuiltInCategory.OST_DuctSystem });

            var mechanicalSystems = new List<VisSystem>();
            mechanicalSystems.AddRange(elements.Select(element => new VisSystem {

                SystemElement = element as MEPSystem,
                SystemSystemName = element.Name,
                SystemFunction = element.GetElementType().GetSharedParamValueOrDefault<string>(
                    _specConfiguration.SystemEF),
                SystemShortName = element.GetElementType().GetSharedParamValueOrDefault<string>(
                    _specConfiguration.SystemShortName),
                SystemTargetName = element.Name.Split(' ').First(),

                SystemForsedInstanceName = element
                .GetSharedParamValueOrDefault<string>(_specConfiguration.ForcedSystemName),
                SystemForcedInstanceFunction = element
                .GetSharedParamValueOrDefault<string>(_specConfiguration.ForcedFunction)
            }));

            return mechanicalSystems;
        }

        /// <summary>
        /// Получаем выбранные элементы по списку категорий
        /// </summary>
        public List<Element> GetSelectedElementsByCategories() {
            var filter = new ElementMulticategoryFilter(_mechanicalCategories);
            var result = new List<Element>();

            var selectedElements = _uidocument.GetSelectedElements();

            var filteredElements = selectedElements
                .Where(e => filter.PassesFilter(e) && FilterElementsToSpecificate(e))
                .ToList();

            // Если выделять объект с вложениями, они не будут выделены. Нужно проверить в выборке на наличие
            // то же самое касается изоляции
            foreach(Element element in filteredElements) {
                if(element is FamilyInstance instance) {
                    List<Element> subElements = DataOperator.GetSub(instance, _document);
                    if(subElements.Count > 0) {
                        result.AddRange(subElements);
                    }
                }

                if(element.InAnyCategory(_elementsCanBeInsulated)) {
                    Element insulation = DataOperator.GetInsulationOfCurve(element, _document);
                    if(insulation != null) {
                        result.Add(insulation);
                    }
                }
            }

            // Добавляем отфильтрованные элементы в результат
            result.AddRange(filteredElements);

            return result;
        }

        /// <summary>
        /// Получаем видимые элементы по списку категорий
        /// </summary>
        public List<Element> GetVisibleElementsByCategories() {
            var filter = new ElementMulticategoryFilter(_mechanicalCategories);
            var view = _document.ActiveView;

            var visibleElements = new FilteredElementCollector(_document, view.Id)
                .WherePasses(filter)
                .WhereElementIsNotElementType()
                .WhereElementIsViewIndependent()
                .ToElements();

            return visibleElements.Where(e => FilterElementsToSpecificate(e)).ToList();
        }

        /// <summary>
        /// Получаем элементы по списку категорий
        /// </summary>
        public List<Element> GetElementsByCategories(List<BuiltInCategory> builtInCategories = null) {
            if(builtInCategories == null) {
                builtInCategories = _mechanicalCategories;
            }
            var filter = new ElementMulticategoryFilter(builtInCategories);
            var elements = (List<Element>) new FilteredElementCollector(_document)
                .WherePasses(filter)
                .WhereElementIsNotElementType()
                .ToElements();
            return elements.Where(e => FilterElementsToSpecificate(e)).ToList();
        }

        /// <summary>
        /// Логический фильтр в зависимости от наличия или отсутствия ФОП_ВИС_Число ДЕ в проекте                        
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private bool FilterElementsToSpecificate(Element element) {
            if(_isNumberCurrencyExists) {
                return FilterElementsWithGroups(element);
            } else {
                return FilterElementsWithoutGroups(element);
            }
        }                       

        /// <summary>
        /// Если ФОП_ВИС_Число ДЕ в проекте - проверяем только на ModelText
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private bool FilterElementsWithGroups(Element element) {
            if(element is ModelText) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Если ФОП_ВИС_Число ДЕ не в проекте - проверяем ModelText и то что элемент не в группе модели
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private bool FilterElementsWithoutGroups(Element element) {
            if(element is ModelText) {
                return false;
            }

            if(element.GroupId.IsNull()) {
                return true;
            }

            return false;
        }
    }
}
