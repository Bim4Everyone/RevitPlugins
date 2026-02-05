using System;
using System.Windows;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SystemParams;
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
            var group = GetGroup(specificationElement);

            TargetParam.Set(group);
        }


        /// <summary>
        /// Базовая + детализированная группа
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private string GetGroup(SpecificationElement specificationElement) {
            var manifold = specificationElement.ManifoldSpElement;
            var targetElement = manifold?.Element ?? specificationElement.Element;

            string baseGroup = GetBaseGroup(targetElement);
            string detailedGroup = manifold == null
                ? GetDetailedGroup(specificationElement)
                : GetManifoldElementDetailedGroup(specificationElement);

            return $"{baseGroup}{detailedGroup}";
        }

        /// <summary>
        /// Получаем ФОП_ВИС_Группирование для узла. Базовая группа + имя семейства-типа узла + 
        /// детальная группа узла+ детальная группа элемента
        /// </summary>
        /// <param name="familyInstance"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        private string GetManifoldElementDetailedGroup(SpecificationElement specificationElement) {
            string manifoldFamylyTypeName = specificationElement.ManifoldInstance
                .GetParam(BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM).AsValueString();

            return
                    $"{GetDetailedGroup(specificationElement.ManifoldSpElement)}" +
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
            string forcedGroup = element.GetSharedParamValueOrDefault(Config.ForcedGroup, string.Empty);

            if(forcedGroup != string.Empty) {
                return $"{forcedGroup}_";
            }

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
            // Имя типоразмера имеет значение только для узлов. Обычные элементы могут быть разного типа, но с одинаковыми экземплярными параметрами. 
            if(specificationElement.ElementType.GetParamValueOrDefault<int>(Config.IsManiFoldParamName, 0) == 1) {                
                RevitParam familyTypeParam = SystemParamsConfig.Instance.CreateRevitParam(
                    Document, 
                    BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM);
                famylyTypeName = specificationElement.Element.GetParamValueString(familyTypeParam);
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
