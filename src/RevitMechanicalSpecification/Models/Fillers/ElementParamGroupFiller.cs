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

        private readonly NameGroupFactory _nameAndGroupFactory;

        public ElementParamGroupFiller(
            string toParamName, 
            string fromParamName, 
            SpecConfiguration specConfiguration, 
            Document document,
            NameGroupFactory nameAndGroupFactory) : 
            base(toParamName, fromParamName, specConfiguration, document) 
            
            {
            _nameAndGroupFactory = nameAndGroupFactory;
        }

        public override void SetParamValue(Element element) {
            TargetParameter.Set(ManifoldInstance != null
                ? _nameAndGroupFactory.GetManifoldGroup(ManifoldInstance, element)
                : element.GetSharedParamValueOrDefault(Config.ForcedGroup, _nameAndGroupFactory.GetGroup(element)));
        }
    }
}
