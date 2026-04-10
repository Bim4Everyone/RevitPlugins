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

        public bool IsForcedSystemPattern(string forcedSystemValue) {
            return TryGetForcedSystemSelector(forcedSystemValue, out _);
        }

        /// <summary>
        /// Возвращает функцию системы
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public string GetFunctionNameValue(SpecificationElement specificationElement) {
            return GetFunctionNameValue(GetVisSystem(specificationElement));
        }

        public string GetFunctionNameValue(Element element) {
            return GetFunctionNameValue(GetVisSystem(element, null));
        }

        private string GetFunctionNameValue(VisSystem visSystem) {
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
        public string GetSystemNameValue(SpecificationElement specificationElement) {
            return GetSystemNameValue(GetVisSystem(specificationElement));
        }

        public string GetSystemNameValue(Element element) {
            return GetSystemNameValue(GetVisSystem(element, null));
        }

        private string GetSystemNameValue(VisSystem visSystem) {
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
        private VisSystem GetVisSystem(SpecificationElement specificationElement) {
            if(specificationElement == null) {
                return null;
            }

            string forcedSystemSelector = GetForcedSystemSelector(specificationElement)
                ?? GetForcedSystemSelector(specificationElement.InsulationSpHost);

            return GetVisSystem(specificationElement.Element, forcedSystemSelector);
        }

        private VisSystem GetVisSystem(Element element, string forcedSystemSelector) {
            if(element == null) {
                return null;
            }

            if(element is FamilyInstance instance) {
                element = instance.GetSuperComponentIfExist();
            }

            List<string> systemNames = GetSystemNames(element);
            if(!string.IsNullOrEmpty(forcedSystemSelector)) {
                string selectedSystemName = systemNames.FirstOrDefault(systemName =>
                    systemName.IndexOf(forcedSystemSelector, StringComparison.OrdinalIgnoreCase) >= 0);
                VisSystem selectedSystem = GetVisSystemByName(selectedSystemName);
                if(selectedSystem != null) {
                    return selectedSystem;
                }
            }

            return systemNames.Select(GetVisSystemByName).FirstOrDefault(visSystem => visSystem != null);
        }

        private List<string> GetSystemNames(Element element) {
            string systemNames = element.InAnyCategory(new HashSet<BuiltInCategory>() {
                BuiltInCategory.OST_DuctInsulations,
                BuiltInCategory.OST_PipeInsulations})
                ? GetInsulationSystem(element as InsulationLiningBase)
                : GetParamSystemValue(element);

            return systemNames
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(item => item.Trim())
                .Where(item => !string.IsNullOrEmpty(item))
                .ToList();
        }

        private VisSystem GetVisSystemByName(string systemName) {
            if(string.IsNullOrWhiteSpace(systemName)) {
                return null;
            }

            return _systems.FirstOrDefault(s => string.Equals(s.SystemSystemName, systemName, StringComparison.Ordinal));
        }

        private string GetForcedSystemSelector(SpecificationElement specificationElement) {
            if(specificationElement == null) {
                return null;
            }

            string forcedSystemValue = GetForcedParamValue(specificationElement, _specConfiguration.ForcedSystemName);
            return TryGetForcedSystemSelector(forcedSystemValue, out string forcedSystemSelector)
                ? forcedSystemSelector
                : null;
        }

        private bool TryGetForcedSystemSelector(string forcedSystemValue, out string forcedSystemSelector) {
            forcedSystemSelector = null;

            if(string.IsNullOrWhiteSpace(forcedSystemValue)) {
                return false;
            }

            string trimmedValue = forcedSystemValue.Trim();
            if(trimmedValue.Length < 3 || trimmedValue[0] != '{' || trimmedValue[trimmedValue.Length - 1] != '}') {
                return false;
            }

            forcedSystemSelector = trimmedValue.Substring(1, trimmedValue.Length - 2).Trim();
            return !string.IsNullOrEmpty(forcedSystemSelector);

        }
    }
}
