using System;
using System.Windows;

using Autodesk.Revit.DB;

using dosymep.Revit;

using Ninject.Planning.Targets;

using RevitMechanicalSpecification.Entities;
using RevitMechanicalSpecification.Service;

namespace RevitMechanicalSpecification.Models.Fillers {
    internal class ElementParamGroupFiller : ElementParamFiller {
        public ElementParamGroupFiller(
            string toParamName,
            string fromParamName,
            SpecConfiguration specConfiguration,
            Document document) :
            base(toParamName, fromParamName, specConfiguration, document) {
        }

        /// <summary>
        /// Устанавливает значение ФОП_ВИС_Группирование в зависимости от того узел это или нет
        /// </summary>
        /// <param name="specificationElement"></param>
        public override void SetParamValue(SpecificationElement specificationElement) {
            var group = specificationElement.ManifoldSpElement != null
                ? GetManifoldGroup(specificationElement)
                : specificationElement.Element
                    .GetSharedParamValueOrDefault(Config.ForcedGroup, GetGroup(specificationElement));

            
            TargetParam.Set(group);
        }

        /// <summary>
        /// Базовая + детализированная группа
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private string GetGroup(SpecificationElement specificationElement) {
            return $"{GetBaseGroup(specificationElement.Element)}{GetDetailedGroup(specificationElement)}";
        }

        /// <summary>
        /// Получаем ФОП_ВИС_Группирование для узла. Базовая группа + имя семейства-типа узла + 
        /// детальная группа узла+ детальная группа элемента
        /// </summary>
        /// <param name="familyInstance"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        private string GetManifoldGroup(SpecificationElement specificationElement) {
            string manifoldFamylyTypeName = specificationElement.ManifoldInstance
                .GetParam(BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM).AsValueString();

            return
                    $"{GetBaseGroup(specificationElement.ManifoldSpElement.Element)}" +
                    $"{GetDetailedGroup(specificationElement.ManifoldSpElement)}"+
                    $"_Узел_" +
                    $"{manifoldFamylyTypeName}" +
                    $"{GetDetailedGroup(specificationElement)}";
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
                    return "1. Оборудование_";
                case BuiltInCategory.OST_PlumbingFixtures:
                    return "1. Оборудование_";
                case BuiltInCategory.OST_Sprinklers:
                    return "1. Оборудование_";
                case BuiltInCategory.OST_DuctAccessory:
                    return "2. Арматура воздуховодов_";
                case BuiltInCategory.OST_DuctTerminal:
                    return "3. Воздухораспределители_";
                case BuiltInCategory.OST_DuctCurves:
                    return "4. Воздуховоды_";
                case BuiltInCategory.OST_FlexDuctCurves:
                    return "4. Гибкие воздуховоды_";
                case BuiltInCategory.OST_DuctFitting:
                    return "5. Фасонные детали воздуховодов_";
                case BuiltInCategory.OST_DuctInsulations:
                    return "6. Материалы изоляции воздуховодов_";
                case BuiltInCategory.OST_PipeAccessory:
                    return "7. Трубопроводная арматура_";
                case BuiltInCategory.OST_PipeCurves:
                    return "8. Трубопроводы_";
                case BuiltInCategory.OST_FlexPipeCurves:
                    return "9. Гибкие трубопроводы_";
                case BuiltInCategory.OST_PipeFitting:
                    return "10. Фасонные детали трубопроводов_";
                case BuiltInCategory.OST_PipeInsulations:
                    return "11. Материалы трубопроводной изоляции_";
                case BuiltInCategory.OST_CableTray:
                    return "99. Кабельные лотки_";
            }

            return "Неизвестная категория";
        }

        /// <summary>
        /// Получаем детализированную группу - имя + марка + код + завод
        /// </summary>
        private string GetDetailedGroup(SpecificationElement specificationElement) {
            string name = specificationElement.ElementName;
            string famylyTypeName = string.Empty;
            // Имя типоразмера имеет значение только для узлов. Обычные элементы могут быть разного типа, но одинакового типоразмера. 
            if(specificationElement.ElementType.IsExistsParam(Config.IsManiFoldParamName)
                && specificationElement.ElementType.GetParamValue<int>(Config.IsManiFoldParamName) == 1) {
                famylyTypeName = specificationElement.Element.GetParam(BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM).AsValueString();
            }
            string mark = !string.IsNullOrEmpty(specificationElement.ElementMark)
                   ? specificationElement.ElementMark
                   : specificationElement.GetTypeOrInstanceParamStringValue(Config.OriginalParamNameMark);

            string code = specificationElement.GetTypeOrInstanceParamStringValue(Config.OriginalParamNameCode);
            string creator = specificationElement.GetTypeOrInstanceParamStringValue(Config.OriginalParamNameCreator);

            return $"{famylyTypeName}_{name}_{mark}_{code}_{creator}";
        }
    }
}
