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

        public ElementParamNumberFiller(string toParamName, 
            string fromParamName, 
            SpecConfiguration specConfiguration,
            Document document) : 
            base(toParamName, fromParamName, specConfiguration, document) 
            {
        }

        private bool LinearLogicalFilter(Element element) 
            {
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
            try {
                VisElementsCalculator calculator = new VisElementsCalculator(Config, Document);
                UnitConverter converter = new UnitConverter();

                unit = element.GetSharedParamValue<string>(Config.TargetNameUnit);
                //расписать почему что возвращается
                if(unit == Config.SingleUnit || unit == Config.KitUnit) { return 1; }
                if(unit == Config.SquareUnit) {
                    if(element.Category.IsId(BuiltInCategory.OST_DuctInsulations)) {
                        InsulationLiningBase insulation = element as InsulationLiningBase;
                        Element host = Document.GetElement(insulation.HostElementId);
                        return calculator.GetFittingArea(host);

                    }
                    if(element.Category.IsId(BuiltInCategory.OST_DuctFitting)) { return calculator.GetFittingArea(element); }
                    if(LinearLogicalFilter(element)) { return converter.DoubleToSquareMeters(element.GetParamValueOrDefault<double>(BuiltInParameter.RBS_CURVE_SURFACE_AREA)); }
                }
                if(unit == Config.MeterUnit) {
                    if(LinearLogicalFilter(element)) {
                        return converter.DoubleToMeters(element.GetParamValueOrDefault<double>(BuiltInParameter.CURVE_ELEM_LENGTH));
                    }
                }
            } 
            catch { MessageBox.Show(element.Id.ToString()); }
            
            return number;
        }


        public override void SetParamValue(Element element) {
            if(element.IsExistsParam(Config.TargetNameUnit)) 
                {
                ToParam.Set(GetNumber(element));
            }
            
        }
    }
}
