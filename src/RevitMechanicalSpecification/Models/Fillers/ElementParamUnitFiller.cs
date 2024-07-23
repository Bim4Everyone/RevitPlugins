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

        private readonly bool _isSpecifyDuctFittings;


        private string DefaultCheck(Element element, string defaultUnit) {
            string unit = GetTypeOrInstanceParamValue(element);
            if(unit == null) { return defaultUnit; }

            if(_linearNames.Contains(unit)) { return unit; }
            if(_squareNames.Contains(unit)) { return unit; }
            if(_singleNames.Contains(unit)) { return unit; }
            if(_kitNames.Contains(unit)) { return unit; }
            return defaultUnit;
        }

        private string GetUnit(Element element, bool isSpecifyDuctFittings) {
            if(element.Category.IsId(BuiltInCategory.OST_DuctCurves)
                || element.Category.IsId(BuiltInCategory.OST_PipeCurves)
                || element.Category.IsId(BuiltInCategory.OST_FlexDuctCurves)
                || element.Category.IsId(BuiltInCategory.OST_FlexPipeCurves)) { return DefaultCheck(element, "м.п."); }

            if(element.Category.IsId(BuiltInCategory.OST_DuctFitting)) {
                if(isSpecifyDuctFittings) { return "шт."; }
                return "м²";
            }

            if(element.Category.IsId(BuiltInCategory.OST_DuctInsulations)) { return DefaultCheck(element, "м²"); }
            if(element.Category.IsId(BuiltInCategory.OST_PipeInsulations)) { return DefaultCheck(element, "м.п."); }

            return "шт.";
        }


        public ElementParamUnitFiller(string toParamName, string fromParamName, bool isSpecifyDuctFittings) : base(toParamName, fromParamName) {
            _isSpecifyDuctFittings = isSpecifyDuctFittings;
        }

        public override void SetParamValue(Element element) {
            if(!element.GetSharedParam(ToParamName).IsReadOnly) { element.GetSharedParam(ToParamName).Set(GetUnit(element, _isSpecifyDuctFittings)); }
        }
    }
}
