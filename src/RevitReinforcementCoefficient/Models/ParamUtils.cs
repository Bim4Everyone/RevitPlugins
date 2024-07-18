using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Revit;

using RevitReinforcementCoefficient.Models.Report;

namespace RevitReinforcementCoefficient.Models {
    internal class ParamUtils {
        private readonly IReportService _reportService;

        public ParamUtils(IReportService reportService) {
            _reportService = reportService;
        }

        /// <summary>
        /// Проверяет есть ли указанный список параметров в элементе на экземпляре или типе, возвращает отчет
        /// </summary>
        public bool HasParamsAnywhere(Element element, List<string> paramNames) {
            foreach(string paramName in paramNames) {
                if(!HasParamAnywhere(element, paramName)) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Проверяет есть ли указанный параметр в элементе на экземпляре или типе
        /// </summary>
        public bool HasParamAnywhere(Element element, string paramName) {
            // Сначала проверяем есть ли параметр на экземпляре
            if(!element.IsExistsParam(paramName)) {
                // Если не нашли, ищем на типоразмере
                Element elementType = element.GetElementType();

                if(!elementType.IsExistsParam(paramName)) {
                    // Если не нашли записываем в отчет и возвращаем false
                    _reportService.AddReportItem(paramName, element.Id);
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
                Element elementType = element.GetElementType();
                return elementType.GetParamValue<T>(paramName);
            }
        }
    }
}
