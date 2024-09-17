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
    internal class ElementParamNumberFiller : ElementParamFiller {
        private readonly VisElementsCalculator _calculator;
        public ElementParamNumberFiller(string toParamName,
            string fromParamName,
            SpecConfiguration specConfiguration,
            Document document
            ) :
            base(toParamName, fromParamName, specConfiguration, document) {
            _calculator = new VisElementsCalculator(Config, Document);
        }

        public override void SetParamValue(SpecificationElement specificationElement) {
            TargetParameter.Set(GetNumber(specificationElement));
        }

        /// <summary>
        /// Получает число в зависимости от единицы измерения
        /// </summary>
        /// <param name="specificationElement"></param>
        /// <returns></returns>
        private double GetNumber(SpecificationElement specificationElement) {
            string unit = specificationElement.Element.GetSharedParamValue<string>(Config.TargetNameUnit);

            // Если единица измерения штуки или комплекты - возвращаем 1
            if(unit == Config.SingleUnit || unit == Config.KitUnit) {
                return 1;
            }

            // Если единица измерения метры квадратные - забираем или высчитываем (для фитингов) площадь в дабл и переводим в метры квадратные
            if(unit == Config.SquareUnit) {
                if(specificationElement.BuiltInCategory == BuiltInCategory.OST_DuctInsulations) {
                    InsulationLiningBase insulation = specificationElement.Element as InsulationLiningBase;
                    Element host = Document.GetElement(insulation.HostElementId);
                    if(host == null) {
                        return 0;
                    }
                    if(host.Category.IsId(BuiltInCategory.OST_DuctFitting)) {
                        double area = _calculator.GetFittingArea(host);
                        return Math.Round(
                            UseStock(host, host.GetElementType(), BuiltInCategory.OST_DuctFitting, area), 2);
                    }
                }

                if(specificationElement.BuiltInCategory == BuiltInCategory.OST_DuctFitting) {
                    double area = _calculator.GetFittingArea(specificationElement.Element);
                    return Math.Round(
                        UseStock(
                            specificationElement.Element,
                            specificationElement.ElementType,
                            specificationElement.BuiltInCategory,
                            area), 2);
                }

                if(LinearLogicalFilter(specificationElement.Element)) {
                    double area = specificationElement.Element.GetParamValueOrDefault<double>(
                        BuiltInParameter.RBS_CURVE_SURFACE_AREA);
                    double convArea = UnitUtils.ConvertFromInternalUnits(area, UnitTypeId.SquareMeters);
                    return Math.Round(
                        UseStock(
                            specificationElement.Element,
                            specificationElement.ElementType,
                            specificationElement.BuiltInCategory,
                            convArea), 2);
                }
            }

            // Если единица измерения метры погонные - забираем длину и переводим в метры
            if(unit == Config.MeterUnit && LinearLogicalFilter(specificationElement.Element)) {
                double len = UnitConverter.DoubleToMeters(specificationElement.Element
                    .GetParamValueOrDefault<double>(BuiltInParameter.CURVE_ELEM_LENGTH));
                return Math.Round(
                    UseStock(
                        specificationElement.Element,
                        specificationElement.ElementType,
                        specificationElement.BuiltInCategory,
                        len), 2);
            }
            return 0;
        }

        /// <summary>
        /// Возвращает True если элемент является линейным
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private bool LinearLogicalFilter(Element element) {
            return element.InAnyCategory(new List<BuiltInCategory>() { BuiltInCategory.OST_DuctCurves,
                        BuiltInCategory.OST_FlexDuctCurves,
                        BuiltInCategory.OST_PipeCurves,
                        BuiltInCategory.OST_FlexPipeCurves,
                        BuiltInCategory.OST_DuctInsulations,
                        BuiltInCategory.OST_PipeInsulations  });
        }

        /// <summary>
        /// Возвращает число, домноженное на запас для конкретной категории
        /// </summary>
        /// <param name="element"></param>
        /// <param name="elemType"></param>
        /// <param name="builtInCategory"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        private double UseStock(Element element, Element elemType, BuiltInCategory builtInCategory ,double number) {

            double individualStock = element.GetTypeOrInstanceParamDoubleValue(elemType, Config.IndividualStock);
            if(individualStock > 0) {
                return number * (1 + individualStock/100);
            }

            switch(builtInCategory) {
                case BuiltInCategory.OST_DuctInsulations:
                    return number * Config.DuctInsulationStock;
                case BuiltInCategory.OST_PipeInsulations:
                    return number * Config.PipeInsulationStock;
                case BuiltInCategory.OST_PipeCurves:
                    return number * Config.DuctAndPipeStock;
                case BuiltInCategory.OST_DuctCurves:
                    return number * Config.DuctAndPipeStock;
                case BuiltInCategory.OST_DuctFitting:
                    return number * Config.DuctAndPipeStock;
                default:
                    return number;
            }
        }
    }
}
