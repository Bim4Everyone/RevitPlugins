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
        private readonly SystemAndFunctionNameFactory _nameFactory;


        public ElementParamFunctionFiller(
            string toParamName, 
            string fromParamName, 
            SpecConfiguration specConfiguration, 
            Document document,
            List<VisSystem> systemList) : 
            base(toParamName, fromParamName, specConfiguration, document) {

            _systemList = systemList;
            _nameFactory = new SystemAndFunctionNameFactory(Document, _systemList);
        }

        private string GetFunction(Element element) {
            return _nameFactory.GetFunctionValue(element);
        }

        public override void SetParamValue(Element element) {
            ToParam.Set(element.GetSharedParamValueOrDefault(Config.ForcedFunction, GetFunction(element)));




        }
    }
}
