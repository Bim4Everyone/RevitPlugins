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

        private readonly HashSet<string> _linearNames = new HashSet<string>() { "м.п.", "м.", "мп", "м", "м.п" };

        private readonly HashSet<string> _singleNames = new HashSet<string>() { "шт", "шт.", "Шт", "Шт.", "Ш", "ш" };

        private readonly HashSet<string> _squareNames = new HashSet<string>() { "м2", "м²" };



        private string DefaultCheck(Element element, string defaultUnit) {
            string unit = GetTypeOrInstanceParamValue(element);

            if(unit == null) { return defaultUnit; }

            if(_linearNames.Contains(unit)) { return Config.MeterUnit; }
            if(_squareNames.Contains(unit)) { return Config.SquareUnit; }
            if(_singleNames.Contains(unit)) { return Config.SingleUnit; }
            if(_kitNames.Contains(unit)) { return Config.SingleUnit; }
            return defaultUnit;
        }

        public string GetUnit(Element element) {
            if(element.InAnyCategory(new List<BuiltInCategory>() { 
                BuiltInCategory.OST_DuctCurves, 
                BuiltInCategory.OST_PipeCurves, 
                BuiltInCategory.OST_FlexDuctCurves, 
                BuiltInCategory.OST_FlexPipeCurves }))
                { return DefaultCheck(element, Config.MeterUnit); }

            if(element.Category.IsId(BuiltInCategory.OST_DuctFitting)) {
                if(Config.IsSpecifyDuctFittings) { return Config.SingleUnit; }
                return Config.SquareUnit;
            }

            if(element.Category.IsId(BuiltInCategory.OST_DuctInsulations)) 
                { return DefaultCheck(element, Config.SquareUnit); }
            if(element.Category.IsId(BuiltInCategory.OST_PipeInsulations)) 
                { return DefaultCheck(element, Config.MeterUnit); }

            return Config.SingleUnit;
        }


        public ElementParamUnitFiller(
            string toParamName, 
            string fromParamName, 
            SpecConfiguration specConfiguration,
            Document document) : 
            base(toParamName, fromParamName, specConfiguration, document) {
        }

        public override void SetParamValue(Element element) {
            ToParam.Set(GetUnit(element)); 
        }
    }
}
