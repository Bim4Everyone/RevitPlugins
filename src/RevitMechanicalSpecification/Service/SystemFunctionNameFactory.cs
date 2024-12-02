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

using RevitMechanicalSpecification.Entities;
using RevitMechanicalSpecification.Models;

namespace RevitMechanicalSpecification.Service {
    internal class SystemFunctionNameFactory {
        private readonly Document _document;
        private readonly List<VisSystem> _systems;
        private readonly SpecConfiguration _specConfiguration;
        private readonly string _noneSystemValue = "Нет системы";

        public SystemFunctionNameFactory(Document document, 
            List<VisSystem> systems, 
            SpecConfiguration specConfiguration

            ) {
            _document = document;
            _systems = systems;
            _specConfiguration = specConfiguration;
        }

        /// <summary>
        /// Если есть принудительное значение на типе - возвращаем.
        /// Если нет, но есть суперкомпонент - проверяем принудительное на нем.
        /// </summary>
        /// <param name="specificationElement"></param>
        /// <param name="paraName"></param>
        /// <returns></returns>
        public string GetForcedParamValue(SpecificationElement specificationElement, string paraName) {
            string result = specificationElement.GetTypeOrInstanceParamStringValue(paraName);

            if(!string.IsNullOrEmpty(result)) {
                return result;
            }
            if(specificationElement.Element is FamilyInstance instance) {
                Element superComponent = GetSuperComponentIfExist(instance);
                if(superComponent != null && specificationElement.Element != superComponent) {

                    return DataOperator
                        .GetTypeOrInstanceParamStringValue(superComponent, superComponent.GetElementType(), paraName);
                }
            }

            return result;
        }

        /// <summary>
        /// Возвращает функцию системы
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public string GetFunctionNameValue(Element element) {
            if(element is FamilyInstance instance) {
                element = GetSuperComponentIfExist(instance);
            }

            VisSystem visSystem = GetVisSystem(element);

            if(visSystem == null) {
                return string.Empty;
            }

            if(!string.IsNullOrEmpty(visSystem.SystemForcedInstanceFunction)) {
                return visSystem.SystemForcedInstanceFunction;
            }

            if(string.IsNullOrEmpty(visSystem.SystemFunction)) {
                return string.Empty;
            }

            return visSystem.SystemFunction;
        }

        /// <summary>
        /// Возвращает имя системы
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public string GetSystemNameValue(Element element) {
            if(element is FamilyInstance instance) {
                element = GetSuperComponentIfExist(instance);
            }

            VisSystem visSystem = GetVisSystem(element);
            if(visSystem is null) {
                return string.Empty;
            }

            if(!string.IsNullOrEmpty(visSystem.SystemForsedInstanceName)) {
                return visSystem.SystemForsedInstanceName;
            }

            if(!string.IsNullOrEmpty(visSystem.SystemShortName)) {
                return visSystem.SystemShortName;
            }

            return visSystem.SystemTargetName;
        }

        /// <summary>
        /// Возвращает системное имя системы. Нужно для определения принадлежности изоляции.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private string GetParamSystemValue(Element element) {
            return element.GetParamValueOrDefault
                (BuiltInParameter.RBS_SYSTEM_NAME_PARAM, _noneSystemValue);
        }

        /// <summary>
        /// Получает системное имя системы материала изоляции
        /// </summary>
        /// <param name="insulation"></param>
        /// <returns></returns>
        private string GetInsulationSystem(InsulationLiningBase insulation) {
            Element host = _document.GetElement(insulation.HostElementId);
            // изоляция может баговать и висеть на трубе или воздуховоде не имея хоста
            if (host == null) {
                return _noneSystemValue;
            }

            return GetParamSystemValue(host);
        }

        /// <summary>
        /// Возвращает суперкомпонент, если он есть. Если нет - возвращает сам элемент.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        private FamilyInstance GetSuperComponentIfExist(FamilyInstance instance) {
            if(!(instance.SuperComponent is null)) {
                instance = (FamilyInstance) instance.SuperComponent;
                instance = GetSuperComponentIfExist(instance);
            }

            return instance;
        }

        /// <summary>
        /// Возвращает первую систему из доступных для элемента
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private VisSystem GetVisSystem(Element element) {
            if(element is FamilyInstance instance) {
                element = instance.GetSuperComponentIfExist();
            }

            string systemName = element.GetParamValueOrDefault(BuiltInParameter.RBS_SYSTEM_NAME_PARAM, _noneSystemValue);

            systemName = systemName.Split(',').FirstOrDefault();

            if(element.InAnyCategory(new HashSet<BuiltInCategory>() {
                BuiltInCategory.OST_DuctInsulations,
                BuiltInCategory.OST_PipeInsulations})) {
                systemName = GetInsulationSystem(element as InsulationLiningBase);
            }

            return _systems.Where(s => s.SystemSystemName == systemName).FirstOrDefault();

        }
    }
}
