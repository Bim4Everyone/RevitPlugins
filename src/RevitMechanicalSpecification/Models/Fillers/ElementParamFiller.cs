using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitMechanicalSpecification.Models.Fillers {
    internal class ElementParamFiller : IElementParamFiller {
        private readonly Document _doc;
        public ElementParamFiller(Document doc) {
            this._doc = doc;
        }
        public void Fill(Element element) { }

        public MEPSystem GetSystemElement() {
            return null; 
        }
    }
}
