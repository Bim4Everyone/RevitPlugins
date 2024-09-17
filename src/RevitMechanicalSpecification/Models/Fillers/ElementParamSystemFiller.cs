using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitMechanicalSpecification.Entities;
using RevitMechanicalSpecification.Service;

namespace RevitMechanicalSpecification.Models.Fillers {
    internal class ElementParamSystemFiller : ElementParamFiller {
        private readonly List<VisSystem> _systemList;
        private readonly SystemFunctionFactory _nameFactory;

        public ElementParamSystemFiller(
            string toParamName, 
            string fromParamName, 
            SpecConfiguration specConfiguration, 
            Document document,
            List<VisSystem> systemList) : 
            base(toParamName, fromParamName, specConfiguration, document) 
            { 
            _systemList = systemList;
            _nameFactory = new SystemFunctionFactory(Document, _systemList);
        }

        public override void SetParamValue(SpecificationElement specificationElement) {
            string calculatedSystem = GetSystemName(specificationElement);
            if(!(string.IsNullOrEmpty(calculatedSystem))) {
                TargetParameter.Set(GetSystemName(specificationElement));
                return;
            }

            TargetParameter.Set(Config.GlobalSystem);
        }

        /// <summary>
        /// Получает имя системы или принудительное имя системы
        /// </summary>
        /// <param name="specificationElement"></param>
        /// <returns></returns>
        private string GetSystemName(SpecificationElement specificationElement) {
            string forcedSystem = _nameFactory.GetForcedParamValue(specificationElement, Config.ForcedSystemName);
            if(!string.IsNullOrEmpty(forcedSystem)) {
                return forcedSystem;
            }
            return _nameFactory.GetSystemValue(specificationElement.Element);
        }
    }
}
