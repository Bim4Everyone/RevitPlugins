using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitClashDetective.Models;

namespace RevitClashDetective.Models {
    internal class SetProvider : IProvider {
        public List<Element> GetElements() {
            throw new NotImplementedException();
        }

        public Outline GetOutline(Solid solid) {
            throw new NotImplementedException();
        }

        public List<Solid> GetSolids(Element Element) {
            throw new NotImplementedException();
        }
    }
}
