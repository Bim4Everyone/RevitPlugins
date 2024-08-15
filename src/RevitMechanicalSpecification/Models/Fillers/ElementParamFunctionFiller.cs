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
        private readonly SystemFunctionFactory _nameFactory;

        public ElementParamFunctionFiller(
            string toParamName,
            string fromParamName,
            SpecConfiguration specConfiguration,
            Document document,
            List<VisSystem> systemList) :
            base(toParamName, fromParamName, specConfiguration, document) {

            _systemList = systemList;
            _nameFactory = new SystemFunctionFactory(Document, _systemList);
        }

        private string GetFunction(Element element) {
            string forcedFunction = _nameFactory.GetForcedFunctionValue(element, ElemType, Config.ForcedFunction);
            //if (string.IsNullOrEmpty(forcedFunction)) {
            //    Console.WriteLine("None");
            //}
            //Console.WriteLine(forcedFunction);
            if(!string.IsNullOrEmpty(forcedFunction)) { 
                return forcedFunction;
            }
            return _nameFactory.GetFunctionValue(element);
        }

        public override void SetParamValue(Element element) {
            string calculatedFunction = GetFunction(element);
            if(!(string.IsNullOrEmpty(calculatedFunction))) {
                ToParam.Set(GetFunction(element));
                return;
            }

            ToParam.Set(Config.GlobalFunction);
        }
    }
}
