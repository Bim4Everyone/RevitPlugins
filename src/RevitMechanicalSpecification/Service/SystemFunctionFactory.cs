using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Revit;

using RevitMechanicalSpecification.Entities;

namespace RevitMechanicalSpecification.Service {
    internal class SystemFunctionFactory {
        private readonly Document _document;
        private readonly List<VisSystem> _systems;
        private readonly string _noneSystemValue = "Нет системы";
        

        public SystemFunctionFactory(Document document, List<VisSystem> systems) {
            _document = document;
            _systems = systems;
        }

        private string GetParamSystemValue(Element element) {
            return element.GetParamValueOrDefault
                (BuiltInParameter.RBS_SYSTEM_NAME_PARAM, _noneSystemValue);
        }

        private string GetInsulationSystem(InsulationLiningBase insulation) {

            Element host = _document.GetElement(insulation.HostElementId);
            //изоляция может баговать и висеть на трубе или воздуховоде не имея хоста
            if (host == null) {
                return _noneSystemValue;
            }

            return GetParamSystemValue(host);
        }

        private FamilyInstance GetSuperComponentIfExist(FamilyInstance instance) {
            if(!(instance.SuperComponent is null)) {
                instance = (FamilyInstance) instance.SuperComponent;
                instance = GetSuperComponentIfExist(instance);
            }

            return instance;
        }

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

        public string GetForcedFunctionValue(Element element, Element elemType, string paraName) {
            if(element is FamilyInstance instance) {
                Element superComponent = GetSuperComponentIfExist(instance);
                if(superComponent != null && element != superComponent) {
                    element = superComponent;
                    elemType = superComponent.GetElementType();
                }
            }

            return DataOperator.GetTypeOrInstanceParamStringValue(element, elemType, paraName);
        }

        public string GetFunctionValue(Element element) {
            if(element is FamilyInstance instance) {
                element = GetSuperComponentIfExist(instance);
            }

            VisSystem visSystem = GetVisSystem(element);
            if(visSystem is null) {
                return null;
            }

            if(!string.IsNullOrEmpty(visSystem.SystemFunction)) { 
                return visSystem.SystemFunction; 
            }

            return null;
        }


        public string GetSystemValue(Element element) {

            VisSystem visSystem = GetVisSystem(element);
            if(visSystem is null) {
                return _noneSystemValue;
            }

            if(!string.IsNullOrEmpty(visSystem.SystemShortName)) {
                return visSystem.SystemShortName;
            }

            return visSystem.SystemTargetName;
        }
    }
}
