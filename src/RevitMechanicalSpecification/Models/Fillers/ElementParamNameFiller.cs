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

        private readonly NameAndGroupFactory _nameAndGroupFactory;

        public ElementParamNameFiller(
            string toParamName,
            string fromParamName,
            SpecConfiguration specConfiguration,
            Document document,
            NameAndGroupFactory nameAndGroupFactory) :
            base(toParamName, fromParamName, specConfiguration, document) {
            _nameAndGroupFactory = nameAndGroupFactory;
        }

        public override void SetParamValue(Element element) {
            string name = _nameAndGroupFactory.GetName(element, ElemType);

            ToParam.Set(ManifoldInstance != null
                ? $"‎    {Count}. {name}"
                : element.GetSharedParamValueOrDefault(Config.ForcedName, name));
        }
    }
}
