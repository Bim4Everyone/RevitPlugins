using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Revit;

namespace RevitMechanicalSpecification.Service {
    internal class MaskReplacer {

        private string GetStringValue(Element element, Element elemType, string paramName) {
            return DataOperator.GetTypeOrInstanceParamDoubleValue(element, elemType, paramName).ToString();
        }

        public string ReplaceMask(Element element, string maskName) {
            Element elemType = element.GetElementType();
            string mask = DataOperator.GetTypeOrInstanceParamStringValue(element, elemType, maskName);


            if(mask == null) {
                return null;
            }

            
            string width = GetStringValue(element, elemType, "ADSK_Размер_Ширина");
            string height = GetStringValue(element, elemType, "ADSK_Размер_Высота");
            string lenght = GetStringValue(element, elemType, "ADSK_Размер_Длина");

            string diameter = GetStringValue(element, elemType, "ADSK_Размер_Диаметр");

            if(mask.Contains("ВЫСОТА")){
                mask.Replace("ВЫСОТА", height);
            }
            if(mask.Contains("ДЛИНА")) {
                mask.Replace("ДЛИНА", lenght);
            }
            if(mask.Contains("ШИРИНА")) {
                mask.Replace("ШИРИНА", width);
            }
            if(mask.Contains("ДИАМЕТР")) {
                mask.Replace("ДИАМЕТР", diameter);
            }

            return null;
        }

    }
}
