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
    internal class ElementParamMarkFiller : ElementParamFiller {
        private readonly VisElementsCalculator _calculator;
        public ElementParamMarkFiller(
            string toParamName, 
            string fromParamName, 
            SpecConfiguration specConfiguration, 
            VisElementsCalculator calculator,
            Document document) : 
            base(toParamName, fromParamName, specConfiguration, document) {
            _calculator = calculator;
        }

        private string GetMark(Element element) {
            string mark = element.GetTypeOrInstanceParamStringValue(ElemType, FromParamName);

            if(element.Category.IsId(BuiltInCategory.OST_DuctFitting)) {
                mark =  _calculator.GetDuctFittingMark(element);
            }

            return mark;
        }

        public override void SetParamValue(Element element) {
            ToParam.Set(GetMark(element));
        }
    }
}
