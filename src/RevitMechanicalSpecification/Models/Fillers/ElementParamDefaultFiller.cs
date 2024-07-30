using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitMechanicalSpecification.Models.Fillers {
    public class ElementParamDefaultFiller : ElementParamFiller {
        public ElementParamDefaultFiller(
            string toParamName, 
            string fromParamName, 
            SpecConfiguration specConfiguration,
            Document document) : 
            base(toParamName, fromParamName, specConfiguration, document) {}

        public override void SetParamValue(Element element) {
            //string originalValue = GetTypeOrInstanceParamValue(element);

            //ToParam.Set(originalValue);
            ToParam.Set(FromParam.AsValueString());
        }
        }
    
    }

