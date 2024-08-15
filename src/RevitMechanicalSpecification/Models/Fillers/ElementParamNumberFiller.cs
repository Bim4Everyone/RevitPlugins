using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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

        private double GetNumber(Element element) {
            double number = 0;
            string unit;
            unit = element.GetSharedParamValue<string>(Config.TargetNameUnit);

            //Если единица измерения штуки или комплекты - возвращаем 1
            if(unit == Config.SingleUnit || unit == Config.KitUnit) {
                return 1;
            }
            //Если единица измерения метры квадратные - забираем или высчитываем(для фитингов) площадь в дабл и переводим в метры квадратные
            if(unit == Config.SquareUnit) {
                if(element.Category.IsId(BuiltInCategory.OST_DuctInsulations)) {
                    InsulationLiningBase insulation = element as InsulationLiningBase;
                    Element host = Document.GetElement(insulation.HostElementId);
                    return _calculator.GetFittingArea(host);
                }
                if(element.Category.IsId(BuiltInCategory.OST_DuctFitting)) {
                    return _calculator.GetFittingArea(element);
                }
                if(LinearLogicalFilter(element)) {
                    double area = element.GetParamValueOrDefault<double>(BuiltInParameter.RBS_CURVE_SURFACE_AREA);
                    double convArea = UnitUtils.ConvertFromInternalUnits(area, UnitTypeId.SquareMeters);
                    return Math.Round(convArea, 2);
                }
            }
            //Если единица измерения метры погонные - забираем длину и переводим в метры
            if(unit == Config.MeterUnit) {
                if(LinearLogicalFilter(element)) {
                    return UnitConverter.DoubleToMeters(element.GetParamValueOrDefault<double>(BuiltInParameter.CURVE_ELEM_LENGTH));
                }
            }

            return number;
        }

        public override void SetParamValue(Element element) {
            ToParam.Set(GetNumber(element));
        }
    }
}
