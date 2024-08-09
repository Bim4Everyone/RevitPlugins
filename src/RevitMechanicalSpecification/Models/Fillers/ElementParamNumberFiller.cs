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

        public ElementParamNumberFiller(string toParamName,
            string fromParamName,
            SpecConfiguration specConfiguration,
            Document document) :
            base(toParamName, fromParamName, specConfiguration, document) {
            _calculator = new VisElementsCalculator(Config, Document);
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
            //написать зачем нужно
            double number = 0;
            string unit;
            unit = element.GetSharedParamValue<string>(Config.TargetNameUnit);

            //расписать почему что возвращается
            if(unit == Config.SingleUnit || unit == Config.KitUnit) {
                return 1;
            }
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
                    return UnitConverter.DoubleToSquareMeters(element.GetParamValueOrDefault<double>(BuiltInParameter.RBS_CURVE_SURFACE_AREA));
                }
            }
            if(unit == Config.MeterUnit) {
                if(LinearLogicalFilter(element)) {
                    return UnitConverter.DoubleToMeters(element.GetParamValueOrDefault<double>(BuiltInParameter.CURVE_ELEM_LENGTH));
                }
            }

            return number;
        }


        public override void SetParamValue(Element element) {
            if(element.IsExistsParam(Config.TargetNameUnit)) {
                ToParam.Set(GetNumber(element));
            }

        }
    }
}
