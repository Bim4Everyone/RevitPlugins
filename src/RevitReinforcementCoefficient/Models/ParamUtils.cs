using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Revit;

using RevitReinforcementCoefficient.ViewModels;

namespace RevitReinforcementCoefficient.Models {
    internal class ParamUtils {
        public ParamUtils() { }

        /// <summary>
        /// Проверяет есть ли указанный список параметров в элементе на экземпляре или типе, возвращает отчет
        /// </summary>
        public bool HasParamsAnywhere(Element element, List<string> paramNames, ReportVM report) {
            bool flag = true;
            foreach(string paramName in paramNames) {
                if(!HasParamAnywhere(element, paramName, report)) {
                    flag = false;
                }
            }
            return flag;
        }

        /// <summary>
        /// Проверяет есть ли указанный параметр в элементе на экземпляре или типе
        /// </summary>
        public bool HasParamAnywhere(Element element, string paramName, ReportVM report) {
            // Сначала проверяем есть ли параметр на экземпляре
            if(!element.IsExistsParam(paramName)) {
                // Если не нашли, ищем на типоразмере
                Element elementType = element.Document.GetElement(element.GetTypeId());

                if(!elementType.IsExistsParam(paramName)) {
                    // Если не нашли записываем в отчет и возвращаем false
                    report.Add(paramName, element.Id);
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Получает значение параметра в элементе на экземпляре или типе
        /// </summary>
        public T GetParamValueAnywhere<T>(Element element, string paramName) {
            try {
                return element.GetParamValue<T>(paramName);
            } catch(Exception) {
                Element elementType = element.Document.GetElement(element.GetTypeId());
                return elementType.GetParamValue<T>(paramName);
            }
        }
    }
}
