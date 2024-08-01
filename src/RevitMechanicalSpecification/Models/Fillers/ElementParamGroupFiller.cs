using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitMechanicalSpecification.Service;

namespace RevitMechanicalSpecification.Models.Fillers {
    internal class ElementParamGroupFiller : ElementParamFiller {

        private readonly NameAndGroupFactory _nameAndGroupFactory;
        

        public ElementParamGroupFiller(
            string toParamName, 
            string fromParamName, 
            SpecConfiguration specConfiguration, 
            Document document,
            NameAndGroupFactory nameAndGroupFactory) : 
            base(toParamName, fromParamName, specConfiguration, document) 
            
            {
            _nameAndGroupFactory = nameAndGroupFactory;
        }

        public override void SetParamValue(Element element) {

            if(ManifoldInstance != null) 
                {
                ToParam.Set(_nameAndGroupFactory.GetManifoldGroup(ManifoldInstance, element));
            } 
            else 
            {
                ToParam.Set(element.GetSharedParamValueOrDefault(Config.ForcedGroup, _nameAndGroupFactory.GetGroup(element)));
            }
            
        }
    }
}
