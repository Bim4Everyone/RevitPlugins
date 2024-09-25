using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Revit;

using RevitMechanicalSpecification.Models;

namespace RevitMechanicalSpecification.Service {
    public class MaskReplacer {
        // должно работать только с шаблонизированными семействами, так что оставляем только ADSK_Параметры, объявляем их тут же
        private readonly string _length = "ДЛИНА";
        private readonly string _adskLength = "ADSK_Размер_Длина";
        private readonly string _width = "ШИРИНА";
        private readonly string _adskWidth = "ADSK_Размер_Ширина";
        private readonly string _height = "ВЫСОТА";
        private readonly string _adskHeight = "ADSK_Размер_Высота";
        private readonly string _diameter = "ДИАМЕТР";
        private readonly string _adskDiameter = "ADSK_Размер_Диаметр";

        private readonly SpecConfiguration _specConfiguration;

        public MaskReplacer(SpecConfiguration specConfiguration) {
            _specConfiguration = specConfiguration;
        }

        /// <summary>
        /// Получает текстовое значение параметров размерностей для подстановки в целевые параметры
        /// </summary>
        /// <param name="element"></param>
        /// <param name="elemType"></param>
        /// <param name="paramName"></param>
        /// <returns></returns>
        private string GetStringValue(Element element, Element elemType, string paramName) {
            double value = element.GetTypeOrInstanceParamDoubleValue(elemType, paramName);
            value = UnitConverter.DoubleToMilimeters(value);

            return value.ToString();
        }

        /// <summary>
        /// Замена маской параметров экземпляра ADSK_Наименование и ADSK_Марка
        /// </summary>
        /// <param name="element"></param>
        /// <param name="maskName"></param>
        /// <param name="toParamName"></param>
        /// <returns></returns>
        public string ReplaceMask(Element element, string maskName, string toParamName) {
            Element elemType = element.GetElementType();
            string mask = element.GetTypeOrInstanceParamStringValue(elemType, maskName);

            // Если мы не нашли маску или у нее нет значение, дальнейшие обработки не имеют смысла
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

            // Здесь нужно обновить значение ADSK_Наименование-Марка для шаблонных семейств с масками
            Parameter toParam = element.LookupParameter(toParamName);
            if(toParam != null) {
                if(!toParam.IsReadOnly) {
                    toParam.Set(mask).ToString();
                }
            }

            return mask;
        }

        // Эта часть должна работать на строго шаблонизированных семействах с ADSK_Наименование и ADSK_Марка. Нужно ли 
        // выносить куда-то определения их имен?

        /// <summary>
        /// Вызов замены ADSK_Марка
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public string ReplaceMarkMask(Element element) {
            return ReplaceMask(element, _specConfiguration.MaskMarkName, "ADSK_Марка");
        }

        /// <summary>
        /// Вызов замены параметра ADSK_Наименование
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public string ReplaceNameMask(Element element) {
            return ReplaceMask(element, _specConfiguration.MaskNameName, "ADSK_Наименование");
        }

        /// <summary>
        /// Вызывает замену обоих целевых параметров масками. Возвращает бул, 
        /// чтоб в общей обработке понять была ли замена
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public bool ExecuteReplacment(Element element) {
            string markMask = ReplaceMarkMask(element);
            string nameMask = ReplaceNameMask(element);

            return !string.IsNullOrEmpty(markMask) & !string.IsNullOrEmpty(nameMask);
        }
    }
}