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

        public CollectionFactory(Document doc, SpecConfiguration specConfiguration, UIDocument uIDocument) {
            _document = doc;
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

            var selectedElements = _uidocument.GetSelectedElements();

            var filteredElements = selectedElements
                .Where(e => filter.PassesFilter(e) && ElementNotInGroupOrModelText(e))
                .ToList();

            // Если выделять объект с вложениями, они не будут выделены. Нужно проверить в выборке на наличие
            foreach(Element element in filteredElements) {
                if(element is FamilyInstance instance) {
                    List<Element> subElements = DataOperator.GetSub(instance, _document);
                    if(subElements.Count > 0) {
                        filteredElements.Concat(subElements);
                    }
                }
            }

            return filteredElements;
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

            return visibleElements.Where(e => ElementNotInGroupOrModelText(e)).ToList();
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
            return elements.Where(e => ElementNotInGroupOrModelText(e)).ToList();
        }

        /// <summary>
        /// Логический фильтр на исключение текста модели
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private bool ElementNotInGroupOrModelText(Element element) {

            if(element is ModelText) {
                return false;
            }

            // временное дополнение. Если в проекте есть ФОП_ВИС_Число ДЕ - группы должны обрабатываться.
            if(!_document.IsExistsParam(SharedParamsConfig.Instance.VISSpecNumbersCurrency)) {
                if(element.GroupId.IsNull()) {
                    return true;
                }
                return false;
            }

            return true;
        }
    }
}
