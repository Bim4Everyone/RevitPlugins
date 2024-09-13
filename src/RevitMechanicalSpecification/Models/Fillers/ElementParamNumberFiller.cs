using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitMechanicalSpecification.Service;

namespace RevitMechanicalSpecification.Models.Fillers {
    internal class ElementParamNumberFiller : ElementParamFiller {
        private readonly VisElementsCalculator _calculator;
        private readonly NameGroupFactory _nameAndGroupFactory;
        public ElementParamNumberFiller(string toParamName,
            string fromParamName,
            SpecConfiguration specConfiguration,
            Document document, NameGroupFactory nameAndGroupFactory
            ) :
            base(toParamName, fromParamName, specConfiguration, document) {
            _calculator = new VisElementsCalculator(Config, Document);
            _nameAndGroupFactory = nameAndGroupFactory;
        }

        private bool LinearLogicalFilter(Element element) {
            return element.InAnyCategory(new List<BuiltInCategory>() { BuiltInCategory.OST_DuctCurves,
                        BuiltInCategory.OST_FlexDuctCurves,
                        BuiltInCategory.OST_PipeCurves,
                        BuiltInCategory.OST_FlexPipeCurves,
                        BuiltInCategory.OST_DuctInsulations,
                        BuiltInCategory.OST_PipeInsulations  });
        }

        private double UseStock(Element element, double number) {
            Category category = element.Category;

            double individualStock = element.GetTypeOrInstanceParamDoubleValue(ElemType, Config.IndividualStock);
            if(individualStock > 0) {
                return number * (1 + individualStock/100);
            }

            switch(category.GetBuiltInCategory()) {
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

        private double GetNumber(Element element) {
            string unit = element.GetSharedParamValue<string>(Config.TargetNameUnit);

            // Если единица измерения штуки или комплекты - возвращаем 1
            if(unit == Config.SingleUnit || unit == Config.KitUnit) {
                return 1;
            }

            // Если единица измерения метры квадратные - забираем или высчитываем (для фитингов) площадь в дабл и переводим в метры квадратные
            if(unit == Config.SquareUnit) {
                if(element.Category.IsId(BuiltInCategory.OST_DuctInsulations)) {
                    InsulationLiningBase insulation = element as InsulationLiningBase;
                    Element host = Document.GetElement(insulation.HostElementId);
                    if(host == null) {
                        return 0;
                    }
                    if(host.Category.IsId(BuiltInCategory.OST_DuctFitting)) {
                        double area = _calculator.GetFittingArea(host);
                        return Math.Round(UseStock(host, area), 2);
                    } 
                }

                if(element.Category.IsId(BuiltInCategory.OST_DuctFitting)) {
                    double area = _calculator.GetFittingArea(element);
                    return Math.Round(UseStock(element, area), 2);
                }

                if(LinearLogicalFilter(element)) {
                    double area = element.GetParamValueOrDefault<double>(BuiltInParameter.RBS_CURVE_SURFACE_AREA);
                    double convArea = UnitUtils.ConvertFromInternalUnits(area, UnitTypeId.SquareMeters);
                    return Math.Round(UseStock(element, convArea), 2);
                }
            }

            // Если единица измерения метры погонные - забираем длину и переводим в метры
            if(unit == Config.MeterUnit && LinearLogicalFilter(element)) {
                double len = UnitConverter.DoubleToMeters(element.GetParamValueOrDefault<double>(BuiltInParameter.CURVE_ELEM_LENGTH));
                return Math.Round(UseStock(element, len), 2);
            }

            return 0;
        }

        public override void SetParamValue(Element element) {
            TargetParameter.Set(GetNumber(element));
        }
    }
}
