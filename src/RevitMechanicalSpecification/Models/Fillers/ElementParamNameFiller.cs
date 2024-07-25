using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitMechanicalSpecification.Models.Classes;

namespace RevitMechanicalSpecification.Models.Fillers {
    internal class ElementParamNameFiller : ElementParamFiller {

        private readonly DuctElementsCalculator _calculator;

        public ElementParamNameFiller(
            string toParamName,
            string fromParamName, 
            SpecConfiguration specConfiguration,
            Document document) : 
            base(toParamName, fromParamName, specConfiguration, document) {
            _calculator = new DuctElementsCalculator(Config, Document);
        }

        private string GetDuctName(Element element) 
            {
            return ", с толщиной стенки " +
                    _calculator.GetDuctThikness(element) +
                    " мм, " +
                    element.GetParamValue(BuiltInParameter.RBS_CALCULATED_SIZE);
        }

        private string GetDuctFittingName(Element element) 
            {
            return _calculator.GetFittingName(element);
        }

        private string GetName(Element element) {
            string name = GetTypeOrInstanceParamValue(element);

            if (String.IsNullOrEmpty(name)) 
                { name = "ЗАПОЛНИТЕ НАИМЕНОВАНИЕ"; }
            if(element.Category.IsId(BuiltInCategory.OST_DuctCurves)) {
                { name += GetDuctName(element); }
            }
            if(element.Category.IsId(BuiltInCategory.OST_DuctFitting)) {
                { name = GetDuctFittingName(element); }
            }

            return name;
        }

        public override void SetParamValue(Element element) {
            ToParam.Set(element.GetSharedParamValueOrDefault(Config.ForcedName, GetName(element))); 
        }
    }
}
