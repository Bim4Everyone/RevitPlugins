using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitMechanicalSpecification.Models.Fillers {
    internal class ElementParamDefaultFiller : IElementParamFiller {
        private readonly Document _doc;
        private readonly SpecConfiguration _config;

        public ElementParamDefaultFiller(Document doc, SpecConfiguration config) {
            this._doc = doc;
            this._config = config;
        }

        public void Fill(Element element, string paramName, string value) {
            if(!element.IsExistsParam(paramName) || element.GetParam(paramName).IsReadOnly) {
                return;
            }
            Parameter targetParam = element.GetParam(paramName);

            targetParam.Set(value);
        }
    }
}
