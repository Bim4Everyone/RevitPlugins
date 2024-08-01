using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitMechanicalSpecification.Service;

namespace RevitMechanicalSpecification.Models.Fillers {
    internal class ElementParamNameFiller : ElementParamFiller {

        private readonly VisElementsCalculator _calculator;
        private readonly NameAndGroupFactory _nameAndGroupFactory;

        public ElementParamNameFiller(
            string toParamName,
            string fromParamName, 
            SpecConfiguration specConfiguration,
            Document document,
            NameAndGroupFactory nameAndGroupFactory) : 
            base(toParamName, fromParamName, specConfiguration, document) {
            _calculator = new VisElementsCalculator(Config, Document);
            _nameAndGroupFactory = nameAndGroupFactory;
        }


        public override void SetParamValue(Element element) {
            ToParam.Set(element.GetSharedParamValueOrDefault(Config.ForcedName, _nameAndGroupFactory.GetName(element)));

            if(ManifoldInstance != null) {
                ToParam.Set("‎    " + Count.ToString() + ". " + _nameAndGroupFactory.GetName(element));
            } else {
                ToParam.Set(element.GetSharedParamValueOrDefault(Config.ForcedName, _nameAndGroupFactory.GetName(element)));
                ;
            }

        }
    }
}
