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
    internal class ElementParamGroupFiller : ElementParamFiller {

        private readonly ManiFoldOperator _maniFoldOperator;
        
        public ElementParamGroupFiller(
            string toParamName, 
            string fromParamName, 
            SpecConfiguration specConfiguration, 
            Document document,
            ManiFoldOperator maniFoldOperator) : 
            base(toParamName, fromParamName, specConfiguration, document) 
            
            {
            _maniFoldOperator = maniFoldOperator;
        }

        public override void SetParamValue(Element element) {
            ToParam.Set(element.GetSharedParamValueOrDefault(Config.ForcedGroup, _maniFoldOperator.GetGroup(element)));
        }
    }
}
