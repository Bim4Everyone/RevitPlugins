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
        private readonly ManiFoldOperator _maniFoldOperator;

        public ElementParamNameFiller(
            string toParamName,
            string fromParamName, 
            SpecConfiguration specConfiguration,
            Document document,
            ManiFoldOperator maniFoldOperator) : 
            base(toParamName, fromParamName, specConfiguration, document) {
            _calculator = new DuctElementsCalculator(Config, Document);
            _maniFoldOperator = maniFoldOperator;
        }


        public override void SetParamValue(Element element) {
            ToParam.Set(element.GetSharedParamValueOrDefault(Config.ForcedName, _maniFoldOperator.GetName(element))); 
        }
    }
}
