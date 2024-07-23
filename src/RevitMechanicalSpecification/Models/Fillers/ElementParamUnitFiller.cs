using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitMechanicalSpecification.Models.Fillers {
    internal class ElementParamUnitFiller : ElementParamFiller {
        private readonly List<string> _kitNames = new List<string>() {
            "компл",
            "компл.",
            "Компл.",
            "Компл",
            "к-т",
            "к-т.",
            "К-т",
            "К-т." };

        private readonly List<string> _linearNames = new List<string>() { "м.п.", "м.", "мп", "м", "м.п" };

        private readonly List<string> _singleNames = new List<string>() { "шт", "шт.", "Шт", "Шт.", "Ш", "ш" };

        private readonly List<string> _squareNames = new List<string>() { "м2", "м²" };


        private readonly SpecConfiguration _specConfiguration;


        private string DefaultCheck(Element element, string defaultUnit) {
            string unit = GetTypeOrInstanceParamValue(element);

            if(unit == null) { return defaultUnit; }

            if(_linearNames.Contains(unit)) { return _specConfiguration.MeterUnit; }
            if(_squareNames.Contains(unit)) { return _specConfiguration.SquareUnit; }
            if(_singleNames.Contains(unit)) { return _specConfiguration.SingleUnit; }
            if(_kitNames.Contains(unit)) { return _specConfiguration.SingleUnit; }
            return defaultUnit;
        }

        public string GetUnit(Element element) {
            if(element.Category.IsId(BuiltInCategory.OST_DuctCurves)
                || element.Category.IsId(BuiltInCategory.OST_PipeCurves)
                || element.Category.IsId(BuiltInCategory.OST_FlexDuctCurves)
                || element.Category.IsId(BuiltInCategory.OST_FlexPipeCurves)) { return DefaultCheck(element, _specConfiguration.MeterUnit); }

            if(element.Category.IsId(BuiltInCategory.OST_DuctFitting)) {
                if(_specConfiguration.IsSpecifyDuctFittings) { return _specConfiguration.SingleUnit; }
                return _specConfiguration.SquareUnit;
            }

            if(element.Category.IsId(BuiltInCategory.OST_DuctInsulations)) { return DefaultCheck(element, _specConfiguration.SquareUnit); }
            if(element.Category.IsId(BuiltInCategory.OST_PipeInsulations)) { return DefaultCheck(element, _specConfiguration.MeterUnit); }

            return _specConfiguration.SingleUnit;
        }


        public ElementParamUnitFiller(string toParamName, string fromParamName, SpecConfiguration specConfiguration) : base(toParamName, fromParamName) {
            _specConfiguration = specConfiguration;
        }

        public override void SetParamValue(Element element) {
            if(!element.GetSharedParam(ToParamName).IsReadOnly) { element.GetSharedParam(ToParamName).Set(GetUnit(element)); }
        }
    }
}
