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

        private string GetSystemName(Element element) 
            {
            //return _nameFactory.GetSystemValue(element);

            string forcedSystem = _nameFactory.GetForcedParamValue(element, ElemType, Config.ForcedSystemName);
            if(!string.IsNullOrEmpty(forcedSystem)) {
                return forcedSystem;
            }
            return _nameFactory.GetSystemValue(element);

        }

        //public override void SetParamValue(Element element) {
        //    ToParam.Set(element.GetSharedParamValueOrDefault(Config.ForcedSystemName, GetSystemName(element)));
        //}

        public override void SetParamValue(Element element) {
            string calculatedSystem = GetSystemName(element);
            if(!(string.IsNullOrEmpty(calculatedSystem))) {
                ToParam.Set(GetSystemName(element));
                return;
            }

            ToParam.Set(Config.GlobalSystem);
        }
    }
}
