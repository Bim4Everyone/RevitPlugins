using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitMechanicalSpecification.Models;

namespace RevitMechanicalSpecification.Service {
    internal class GroupFactory {
        private readonly SpecConfiguration _config;

        public GroupFactory(SpecConfiguration config) {
            _config = config;
        }


        /// <summary>
        /// Получаем ФОП_ВИС_Группирование для узла
        /// </summary>
        /// <param name="familyInstance"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        public string GetManifoldGroup(FamilyInstance familyInstance, Element element, string manifoldName, string elementName) {
            return
                    $"{GetBaseGroup(familyInstance)}" +
                    $"{GetDetailedGroup(familyInstance, manifoldName)}" +
                    $"{familyInstance.get_Parameter(BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM).AsValueString()}" +
                    $"_Узел_" +
                    $"_{GetDetailedGroup(element, elementName)}";
        }

        /// <summary>
        /// Получаем базовое группирование элемента
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private string GetBaseGroup(Element element) {
            Category category = element.Category;
            switch(category.GetBuiltInCategory()) {
                case BuiltInCategory.OST_MechanicalEquipment:
                    return "1. Оборудование";
                case BuiltInCategory.OST_PlumbingFixtures:
                    return "1. Оборудование";
                case BuiltInCategory.OST_Sprinklers:
                    return "1. Оборудование";
                case BuiltInCategory.OST_DuctAccessory:
                    return "2. Арматура воздуховодов";
                case BuiltInCategory.OST_DuctTerminal:
                    return "3. Воздухораспределители";
                case BuiltInCategory.OST_DuctCurves:
                    return "4. Воздуховоды";
                case BuiltInCategory.OST_FlexDuctCurves:
                    return "4. Гибкие воздуховоды";
                case BuiltInCategory.OST_DuctFitting:
                    return "5. Фасонные детали воздуховодов";
                case BuiltInCategory.OST_DuctInsulations:
                    return "6. Материалы изоляции воздуховодов";
                case BuiltInCategory.OST_PipeAccessory:
                    return "7. Трубопроводная арматура";
                case BuiltInCategory.OST_PipeCurves:
                    return "8. Трубопроводы";
                case BuiltInCategory.OST_FlexPipeCurves:
                    return "9. Гибкие трубопроводы";
                case BuiltInCategory.OST_PipeFitting:
                    return "10. Фасонные детали трубопроводов";
                case BuiltInCategory.OST_PipeInsulations:
                    return "11. Материалы трубопроводной изоляции";
            }

            return "Неизвестная категория";
        }

        /// <summary>
        /// Получаем детализированную группу - имя + марка + код + завод
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private string GetDetailedGroup(Element element, string name) {
            Element elemType = element.GetElementType();

            string mark = element.GetTypeOrInstanceParamStringValue(elemType, _config.OriginalParamNameMark);
            string code = element.GetTypeOrInstanceParamStringValue(elemType, _config.OriginalParamNameCode);
            string creator = element.GetTypeOrInstanceParamStringValue(elemType, _config.OriginalParamNameCreator);
            return $"_{name}_{mark}_{code}_{creator}";
        }
    }
}
