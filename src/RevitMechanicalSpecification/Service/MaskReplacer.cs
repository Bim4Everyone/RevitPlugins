using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Revit;

namespace RevitMechanicalSpecification.Service {
    public static class MaskReplacer {

        private static string GetStringValue(Element element, Element elemType, string paramName) {
            double value = DataOperator.GetTypeOrInstanceParamDoubleValue(element, elemType, paramName);

            value = UnitConverter.DoubleToMilimeters(value);

            return value.ToString();
        }

        public static string ReplaceMask(Element element, string maskName) {
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
                mask = mask.Replace("ВЫСОТА", height);
            }
            if(mask.Contains("ДЛИНА")) {
                mask = mask.Replace("ДЛИНА", lenght);
            }
            if(mask.Contains("ШИРИНА")) {
                mask = mask.Replace("ШИРИНА", width);
            }
            if(mask.Contains("ДИАМЕТР")) {
                mask = mask.Replace("ДИАМЕТР", diameter);
            }

            return mask;
        }

    }
}
