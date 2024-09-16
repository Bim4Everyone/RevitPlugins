using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitMechanicalSpecification.Entities;
using RevitMechanicalSpecification.Service;

namespace RevitMechanicalSpecification.Models.Fillers {
    internal class ElementParamNameFiller : ElementParamFiller {
        private readonly VisElementsCalculator _calculator;
        

        public ElementParamNameFiller(
            string toParamName,
            string fromParamName,
            SpecConfiguration specConfiguration,
            Document document,
            VisElementsCalculator visElementsCalculator) :
            base(toParamName, fromParamName, specConfiguration, document) {
            _calculator = visElementsCalculator;
        }

        public string GetName(Element element, Element elemType) {
            string name = element.GetTypeOrInstanceParamStringValue(elemType, Config.OriginalParamNameName);
            string nameAddon = element.GetTypeOrInstanceParamStringValue(elemType, Config.NameAddition);

            if(string.IsNullOrEmpty(name)) {
                name = "ЗАПОЛНИТЕ НАИМЕНОВАНИЕ";
            }
            Category category = element.Category;
            switch(category.GetBuiltInCategory()) {
                case BuiltInCategory.OST_DuctCurves:
                    return $"{name} {_calculator.GetDuctName(element, elemType)} {nameAddon}";
                case BuiltInCategory.OST_DuctFitting:
                    return $"{_calculator.GetDuctFittingName(element)} {nameAddon}";
                case BuiltInCategory.OST_PipeFitting:
                    // Если учет фитингов труб отключен в проекте, не учитываем. Если включен в проекте, но выключен в трубе - не учитываем
                    return _calculator.IsSpecifyPipeFittingName(element) ?
                        $"{name} {nameAddon}" : "!Не учитывать";
                case BuiltInCategory.OST_PipeCurves:
                    return $"{name} {_calculator.GetPipeSize(element, elemType)} {nameAddon}";
                case BuiltInCategory.OST_PipeInsulations:
                    InsulationLiningBase insulation = element as InsulationLiningBase;
                    Element pipe = Document.GetElement(insulation.HostElementId);
                    return pipe != null ?
                        $"{name} (Для: {GetName(pipe, pipe.GetElementType())}) {nameAddon}" : "!Не учитывать";
                case BuiltInCategory.OST_DuctAccessory:
                    return $"{name} {nameAddon}";
            }

            return $"{name} {nameAddon}";
        }

        public override void SetParamValue(SpecificationElement specificationElement) {
            string name = GetName(specificationElement.Element, ElemType);

            TargetParameter.Set(specificationElement.ManifoldInstance != null
                ? $"‎    •  {name}"
                : specificationElement.Element.GetSharedParamValueOrDefault(Config.ForcedName, name));

            specificationElement.ElementName = name;
        }
    }
}
