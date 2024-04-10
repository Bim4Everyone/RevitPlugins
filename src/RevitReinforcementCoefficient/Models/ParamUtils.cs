using System;
using System.Collections.Generic;
using System.Text;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Revit;

namespace RevitReinforcementCoefficient.Models {
    internal class ParamUtils {

        public ParamUtils() { }

        /// <summary>
        /// Проверяет есть ли указанный список параметров в элементе на экземпляре или типе, возвращает отчет
        /// </summary>
        public StringBuilder HasParamsAnywhere(Element element, List<string> paramNames, StringBuilder errors = null) {

            if(errors is null) {
                errors = new StringBuilder();
            }

            foreach(string paramName in paramNames) {

                if(!HasParamAnywhere(element, paramName)) {

                    errors.AppendLine($"У элемента с {element.Id} не найден параметр {paramName}");
                }
            }
            return errors;
        }


        /// <summary>
        /// Проверяет есть ли указанный параметр в элементе на экземпляре или типе
        /// </summary>
        public bool HasParamAnywhere(Element element, string paramName) {

            // Сначала проверяем есть ли параметр на экземпляре
            if(!element.IsExistsParam(paramName)) {

                // Если не нашли, ищем на типоразмере
                Element elementType = element.Document.GetElement(element.GetTypeId());

                if(!elementType.IsExistsParam(paramName)) {
                    // Если не нашли записываем, то возвращаем false

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
