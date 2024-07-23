using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitMechanicalSpecification.Models.Classes;

namespace RevitMechanicalSpecification.Models.Fillers {
    internal class ElementParamNumberFiller : ElementParamFiller {
        private readonly SpecConfiguration _specConfiguration;
        private readonly Document _document;

        public ElementParamNumberFiller(string toParamName, 
            string fromParamName, 
            SpecConfiguration specConfiguration,
            Document document) : base(toParamName, fromParamName) 
            {
            _specConfiguration = specConfiguration;
            _document = document;
        }

        private bool LinearLogicalFilter(Element element) 
            {
            if(element.Category.IsId(BuiltInCategory.OST_DuctCurves) ||
                        element.Category.IsId(BuiltInCategory.OST_FlexDuctCurves) ||
                        element.Category.IsId(BuiltInCategory.OST_PipeCurves) ||
                        element.Category.IsId(BuiltInCategory.OST_FlexPipeCurves) ||
                        element.Category.IsId(BuiltInCategory.OST_DuctInsulations) ||
                        element.Category.IsId(BuiltInCategory.OST_PipeInsulations)) { return true; }
            return false;
            }

        private double GetNumber(Element element) {
            double number = 0;
            string unit;
            DuctElementsCalculator calculator = new DuctElementsCalculator();
            UnitConverter converter = new UnitConverter();

            unit = element.GetSharedParamValue<string>(_specConfiguration.TargetNameUnit);

            if(unit == _specConfiguration.SingleUnit || unit == _specConfiguration.KitUnit) { return 1; }
            if(unit == _specConfiguration.SquareUnit) {
                if(element.Category.IsId(BuiltInCategory.OST_DuctInsulations)) 
                    { 
                    InsulationLiningBase insulation =  element as InsulationLiningBase;
                    Element host = _document.GetElement(insulation.HostElementId);
                    return calculator.GetFittingArea(host);
                    
                        }
                if(element.Category.IsId(BuiltInCategory.OST_DuctFitting)) { return calculator.GetFittingArea(element); }
                if(LinearLogicalFilter(element)) 
                    { return converter.DoubleToSquareMeters(element.GetParamValueOrDefault<double>(BuiltInParameter.RBS_CURVE_SURFACE_AREA, 0)); }
            }
            if(unit == _specConfiguration.MeterUnit) {
                if(LinearLogicalFilter(element)) 
                    {
                    return converter.DoubleToMeters(element.GetParamValueOrDefault<double>(BuiltInParameter.CURVE_ELEM_LENGTH, 0)); }
            }
            return number;
        }


        public override void SetParamValue(Element element) {
            if(!element.GetSharedParam(ToParamName).IsReadOnly) { element.GetSharedParam(ToParamName).Set(GetNumber(element)); }
        }
    }
}
