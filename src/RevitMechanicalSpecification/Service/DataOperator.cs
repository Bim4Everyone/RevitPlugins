using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.Bim4Everyone;
using Autodesk.Revit.DB;
using dosymep.Revit;

namespace RevitMechanicalSpecification.Service {
    public static class DataOperator {
        //возвращает значение параметра по типу или экземпляру, если существует, иначе null
        public static string GetTypeOrInstanceParamStringValue(Element element, Element elemType, string paraName) {
            if(element.IsExistsParam(paraName)) {
                return element.GetSharedParamValueOrDefault<string>(paraName);
            }
            if(elemType.IsExistsParam(paraName)) {
                return elemType.GetSharedParamValueOrDefault<string>(paraName);
            }
            return null;
        }

        public static double GetTypeOrInstanceParamDoubleValue(Element element, Element elemType, string paraName) {
            if(element.IsExistsParam(paraName)) {
                return element.GetSharedParamValueOrDefault<double>(paraName);
            }
            if(elemType.IsExistsParam(paraName)) {
                return elemType.GetSharedParamValueOrDefault<double>(paraName);
            }
            return 0;
        }

    }



}
