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

namespace RevitMechanicalSpecification.Service {

    internal class CollectionFactory {
        private readonly Document _document;
        private readonly SpecConfiguration _specConfiguration;

        public CollectionFactory(Document doc, SpecConfiguration specConfiguration) {
            _document = doc;
            _specConfiguration = specConfiguration;
        }

        /// <summary>
        /// Получение специфицируемых элементов
        /// </summary>
        /// <returns></returns>
        public List<Element> GetElementsToSpecificate() {
            var mechanicalCategories = new List<BuiltInCategory>()
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
            return GetElementsByCategories(mechanicalCategories);
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
                SystemTargetName = element.Name.Split(' ').First()
            }));

            return mechanicalSystems;
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

            if(element.GroupId.IsNull()) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Получаем элементы по списку категорий
        /// </summary>
        /// <param name="builtInCategories"></param>
        /// <returns></returns>
        private List<Element> GetElementsByCategories(List<BuiltInCategory> builtInCategories) {
            var filter = new ElementMulticategoryFilter(builtInCategories);
            var elements = (List<Element>) new FilteredElementCollector(_document)
                .WherePasses(filter)
                .WhereElementIsNotElementType()
                .ToElements();
            return elements.Where(e => ElementNotInGroupOrModelText(e)).ToList();
        }
    }
}
