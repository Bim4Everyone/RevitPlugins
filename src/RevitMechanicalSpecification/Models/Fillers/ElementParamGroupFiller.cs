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
            base(toParamName, fromParamName, specConfiguration, document) 
            
            {

        }

        /// <summary>
        /// Базовая + детализированная группа
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public string GetGroup(SpecificationElement specificationElement) {
            string detailedGroup = GetDetailedGroup(specificationElement);
            return $"{GetBaseGroup(specificationElement.Element)}{detailedGroup}";
        }

        /// <summary>
        /// Получаем ФОП_ВИС_Группирование для узла
        /// </summary>
        /// <param name="familyInstance"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        public string GetManifoldGroup(SpecificationElement specificationElement) {
            string manifoldFamylyTypeName = specificationElement.ManifoldInstance
                .get_Parameter(BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM).AsValueString();

            return
                    
                    $"{GetBaseGroup(specificationElement.ManifoldElement)}" +
                    $"{manifoldFamylyTypeName}" +
                    $"{GetDetailedGroup(specificationElement.ManifoldSpElement)}" +
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
        private string GetDetailedGroup(SpecificationElement specificationElement) {
            string name = specificationElement.ElementName;
            string mark = specificationElement.GetTypeOrInstanceParamStringValue(Config.OriginalParamNameMark);
            string code = specificationElement.GetTypeOrInstanceParamStringValue(Config.OriginalParamNameCode);
            string creator = specificationElement.GetTypeOrInstanceParamStringValue(Config.OriginalParamNameCreator);

            return $"_{name}_{mark}_{code}_{creator}";
        }

        public override void SetParamValue(SpecificationElement specificationElement) {
            TargetParameter.Set(specificationElement.ManifoldInstance != null
                ? GetManifoldGroup(specificationElement)
                : specificationElement.Element
                .GetSharedParamValueOrDefault(Config.ForcedGroup, GetGroup(specificationElement)));
        }
    }
}
