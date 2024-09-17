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
                return element.GetSharedParamValueOrDefault<string>(paraName);
            }
            if(elemType.IsExistsParam(paraName)) {
                return elemType.GetSharedParamValueOrDefault<string>(paraName);
            }
            return null;
        }

        /// <summary>
        /// Возвращает сам параметр из типа или экземпляра
        /// </summary>
        /// <param name="specificationElement"></param>
        /// <param name="paraName"></param>
        /// <returns></returns>
        public static string GetTypeOrInstanceParamStringValue(
            this SpecificationElement specificationElement, 
            string paraName) {
            if(specificationElement.Element.IsExistsParam(paraName)) {
                return specificationElement.Element.GetSharedParamValueOrDefault<string>(paraName);
            }
            if(specificationElement.ElementType.IsExistsParam(paraName)) {
                return specificationElement.ElementType.GetSharedParamValueOrDefault<string>(paraName);
            }
            return null;
        }

        public static Parameter GetTypeOrInstanceParam(this SpecificationElement specificationElement, string paramName) {
            if(paramName is null) {
                return null;
            }
            Parameter parameter = specificationElement.Element.LookupParameter(paramName) ?? 
                specificationElement.ElementType.LookupParameter(paramName);
            if(parameter == null) {
                return null;
            }
            return parameter;
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
                return element.GetSharedParamValueOrDefault<double>(paraName);
            }
            if(elemType.IsExistsParam(paraName)) {
                return elemType.GetSharedParamValueOrDefault<double>(paraName);
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

        /// <summary>
        /// Возвращает субкомпоненты и субкомпоненты субкомпонентов
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static List<Element> GetSub(FamilyInstance element, Document document) {
            var subs = new List<Element>();

            foreach(ElementId elementId in element.GetSubComponentIds()) {
                Element subElement = document.GetElement(elementId);
                subs.Add(subElement);

                var subInst = subElement as FamilyInstance;
                if(subInst.GetSubComponentIds().Count > 0) {
                    subs.AddRange(GetSub(subInst, document));
                }
            }
            return subs;
        }
    }
}
