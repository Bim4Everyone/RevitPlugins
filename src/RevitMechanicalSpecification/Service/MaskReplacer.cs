using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Revit;

using RevitMechanicalSpecification.Models;

namespace RevitMechanicalSpecification.Service {
    public static class MaskReplacer {
        //должно работать только с шаблонизированными семействами, так что оставляем только ADSK_Параметры, объявляем их тут же
        private static readonly string _length = "ДЛИНА";
        private static readonly string _adskLength = "ADSK_Размер_Длина";
        private static readonly string _width = "ШИРИНА";
        private static readonly string _adskWidth = "ADSK_Размер_Ширина";
        private static readonly string _height = "ВЫСОТА";
        private static readonly string _adskHeight = "ADSK_Размер_Высота";
        private static readonly string _diameter = "ДИАМЕТР";
        private static readonly string _adskDiameter = "ADSK_Размер_Диаметр";

        private static string GetStringValue(Element element, Element elemType, string paramName) {
            double value = element.GetTypeOrInstanceParamDoubleValue(elemType, paramName);

            value = UnitConverter.DoubleToMilimeters(value);

            return value.ToString();
        }

        public static string ReplaceMask(Element element, string maskName, string toParamName) {
            Element elemType = element.GetElementType();
            string mask = element.GetTypeOrInstanceParamStringValue(elemType, maskName);

            if(mask == null) {
                return null;
            }

            string width = GetStringValue(element, elemType, _adskWidth);
            string height = GetStringValue(element, elemType, _adskHeight);
            string lenght = GetStringValue(element, elemType, _adskLength);

            string diameter = GetStringValue(element, elemType, _adskDiameter);

            if(mask.Contains(_height)) {
                mask = mask.Replace(_height, height);
            }
            if(mask.Contains(_length)) {
                mask = mask.Replace(_length, lenght);
            }
            if(mask.Contains(_width)) {
                mask = mask.Replace(_width, width);
            }
            if(mask.Contains(_diameter)) {
                mask = mask.Replace(_diameter, diameter);
            }

            //Здесь нужно обновить значение ADSK_Наименование-Марка для шаблонных семейств с масками
            Parameter toParam = element.LookupParameter(toParamName);
            if(toParam != null) {
                if(!toParam.IsReadOnly) {
                    toParam.Set(mask).ToString();
                }
            }

            return mask;
        }

    }
}
