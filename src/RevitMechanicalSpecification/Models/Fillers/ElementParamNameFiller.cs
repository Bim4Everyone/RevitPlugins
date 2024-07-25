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
    internal class ElementParamNameFiller : ElementParamFiller {
        
        public ElementParamNameFiller(
            string toParamName,
            string fromParamName, 
            SpecConfiguration specConfiguration,
            Document document) : 
            base(toParamName, fromParamName, specConfiguration, document) {
        }

        private string GetName(Element element) {
            string name = "";
            DuctElementsCalculator calculator = new DuctElementsCalculator(Config, Document);
            name += GetTypeOrInstanceParamValue(element);

            if(element.Category.IsId(BuiltInCategory.OST_DuctCurves)) 
                { name += ", с толщиной стенки " + 
                    calculator.GetDuctThikness(element) + 
                    " мм, " + 
                    element.GetParamValue(BuiltInParameter.RBS_CALCULATED_SIZE); }

            return name;
        }

        public override void SetParamValue(Element element) {
            ToParam.Set(element.GetSharedParamValueOrDefault(Config.ForcedName, GetName(element))); 
        }
    }
}
