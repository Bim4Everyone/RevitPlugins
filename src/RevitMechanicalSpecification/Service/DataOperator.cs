using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.Bim4Everyone;
using Autodesk.Revit.DB;
using dosymep.Revit;
using RevitMechanicalSpecification.Entities;

namespace RevitMechanicalSpecification.Service {
    public static class DataOperator {
        /// <summary>
        /// возвращает значение параметра по типу или экземпляру, если существует, иначе null
        /// </summary>
        /// <param name="element"></param>
        /// <param name="elemType"></param>
        /// <param name="paraName"></param>
        /// <returns></returns>
        public static string GetTypeOrInstanceParamStringValue(this Element element, Element elemType, string paraName) {
            if(element.IsExistsParam(paraName)) {
                return element.GetParamValueOrDefault<string>(paraName);
            }
            if(elemType.IsExistsParam(paraName)) {
                return elemType.GetParamValueOrDefault<string>(paraName);
            }
            return null;
        }

        /// <summary>
        /// возвращает значение параметра по типу или экземпляру, если существует, иначе null
        /// </summary>
        /// <param name="element"></param>
        /// <param name="elemType"></param>
        /// <param name="paraName"></param>
        /// <returns></returns>
        public static string GetTypeOrInstanceParamStringValue(
            this SpecificationElement specificationElement,
            string paraName) {
            if(specificationElement.Element.IsExistsParam(paraName)) {

                return specificationElement.Element.GetParamValue<string>(paraName);
            }
            if(specificationElement.ElementType.IsExistsParam(paraName)) {
                return specificationElement.ElementType.GetParamValue<string>(paraName);
            }
            return null;
        }

        /// <summary>
        /// Возвращает сам параметр из типа или экземпляра
        /// </summary>
        /// <param name="specificationElement"></param>
        /// <param name="paraName"></param>
        /// <returns></returns>
        public static Parameter GetTypeOrInstanceParam(this Element element, Element elementType, string paramName) {
            if(element.IsExistsParam(paramName)) {
                return element.GetParam(paramName);
            }

            if(elementType.IsExistsParam(paramName)) {
                return elementType.GetParam(paramName);
            }

            return null;
        }

        public static bool IsTypeOrInstanceParamExist(this SpecificationElement specificationElement, string paramName) {
            return specificationElement.Element.IsExistsParam(paramName)
                || specificationElement.ElementType.IsExistsParam(paramName);
        }


        /// <summary>
        /// Получаем дабл из параметра в экземпляре или типе, иначе возвращает 0
        /// </summary>
        /// <param name="element"></param>
        /// <param name="elemType"></param>
        /// <param name="paraName"></param>
        /// <returns></returns>
        public static double GetTypeOrInstanceParamDoubleValue(this Element element, Element elemType, string paraName) {
            if(element.IsExistsParam(paraName)) {
                return element.GetParamValueOrDefault<double>(paraName);
            }
            if(elemType.IsExistsParam(paraName)) {
                return elemType.GetParamValueOrDefault<double>(paraName);
            }
            return 0;
        }

        /// <summary>
        /// Если есть суперкомпонент - возвращает его. Иначе возвращает исходник
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static FamilyInstance GetSuperComponentIfExist(this FamilyInstance instance) {
            if(!(instance.SuperComponent is null)) {
                instance = (FamilyInstance) instance.SuperComponent;
                instance = GetSuperComponentIfExist(instance);
            }
            return instance;
        }

        public static Element GetInsulationOfCurve(Element element, Document document) {

            foreach(ElementId elementId in element.GetDependentElements(null)) {
                Element subElement = document.GetElement(elementId);
                if(subElement == null) {
                    continue;
                }

                if(subElement.Category.IsId(BuiltInCategory.OST_PipeInsulations)) {
                    return subElement;
                }

                if(subElement.Category.IsId(BuiltInCategory.OST_DuctInsulations)) {
                    return subElement;
                }
            }
            return null;
        }

        /// <summary>
        /// Возвращает субкомпоненты и субкомпоненты субкомпонентов
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static List<Element> GetSub(FamilyInstance element, Document document) {
            var subs = new List<Element>();

            foreach(ElementId elementId in element.GetSubComponentIds()) {
                Element subElement = document.GetElement(elementId);
                if(subElement == null) {
                    continue;
                }
                subs.Add(subElement);

                if(subElement is FamilyInstance subInst && subInst.GetSubComponentIds().Count > 0) {
                    subs.AddRange(GetSub(subInst, document));
                }
            }
            return subs;
        }
    }
}
