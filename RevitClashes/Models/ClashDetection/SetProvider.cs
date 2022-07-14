using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitClashDetective.Models;

namespace RevitClashDetective.Models {
    internal class SetProvider : IProvider {
        public Document Doc => throw new NotImplementedException();

        public Transform MainTransform => throw new NotImplementedException();

        public List<Element> GetElements() {
            throw new NotImplementedException();
        }
        public List<Solid> GetSolids(Element Element) {
            throw new NotImplementedException();
        }
    }
}
