using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitValueModifier.Models {
    internal class RevitElem {

        public RevitElem(Element element, List<ParamValuePair> paramValuePair) {
            Element = element;
            ParamValuePairs = paramValuePair;
        }

        public Element Element { get; }
        public List<ParamValuePair> ParamValuePairs { get; }
    }
}
