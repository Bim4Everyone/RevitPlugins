using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitMechanicalSpecification.Entities;
using RevitMechanicalSpecification.Service;

namespace RevitMechanicalSpecification.Models.Fillers {
    internal class ElementParamFunctionFiller : ElementParamFiller {
        private readonly List<VisSystem> _systemList;
        private readonly SystemFunctionNameFactory _nameFactory;

        public ElementParamFunctionFiller(
            string toParamName,
            string fromParamName,
            SpecConfiguration specConfiguration,
            Document document,
            List<VisSystem> systemList) :
            base(toParamName, fromParamName, specConfiguration, document) {

            _systemList = systemList;
            _nameFactory = new SystemFunctionNameFactory(Document, _systemList);
        }

        public override void SetParamValue(SpecificationElement specificationElement) {
            string calculatedFunction = GetFunction(specificationElement);
            if(!(string.IsNullOrEmpty(calculatedFunction))) {
                TargetParam.Set(GetFunction(specificationElement));
                return;
            }
            TargetParam.Set(Config.GlobalFunction);
        }

        /// <summary>
        /// Возвращает экономическую или принудительную экономическую функцию
        /// </summary>
        /// <param name="specificationElement"></param>
        /// <returns></returns>
        private string GetFunction(SpecificationElement specificationElement) {
            string forcedFunction = _nameFactory.GetForcedParamValue(specificationElement, Config.ForcedFunction);
            if(!string.IsNullOrEmpty(forcedFunction)) {
                return forcedFunction;
            }
            return _nameFactory.GetFunctionNameValue(specificationElement.Element);
        }
    }
}
